using Azure.Core;
using Dapper;
using DateTimeService.Cache;
using DateTimeService.Controllers;
using DateTimeService.Data;
using DateTimeService.Exceptions;
using DateTimeService.Models;
using DateTimeService.Models.AvailableDeliveryTypes;
using Hangfire.MemoryStorage.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DateTimeService.Services
{
    public class DataService : IDataService
    {
        private readonly IConfiguration _configuration;
        private readonly ILoadBalancing _loadBalancing;
        private readonly ILogger<DateTimeController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IGeoZones _geoZones;
        private readonly IMemoryCache _memoryCache;
        private readonly ConnectionMultiplexer _redis;
        private readonly RedisSettings _redisSettings;

        public DataService(IConfiguration configuration, ILoadBalancing loadBalancing, ILogger<DateTimeController> logger,
            IHttpContextAccessor contextAccessor, IGeoZones geoZones, IMemoryCache memoryCache, ConnectionMultiplexer redis, RedisSettings redisSettings)
        {
            _configuration = configuration;
            _loadBalancing = loadBalancing;
            _logger = logger;
            _contextAccessor = contextAccessor;
            _geoZones = geoZones;
            _memoryCache = memoryCache;
            _redis = redis;
            _redisSettings = redisSettings;
        }

        public async Task<ResponseAvailableDeliveryTypes> GetAvailableDeliveryTypes(RequestAvailableDeliveryTypes inputData, CancellationToken token = default)
        {
            var result = new ResponseAvailableDeliveryTypes();

            Task<DeliveryTypeAvailabilityResult> taskSelf;
            Task<DeliveryTypeAvailabilityResult> taskCourier;
            Task<DeliveryTypeAvailabilityResult> taskYourTime;

            if (_configuration.GetValue<bool>("UseDapper"))
            {
                taskSelf = Task.Run(() => GetDeliveryTypeAvailabilityDapper(inputData, Constants.Self, token));
                taskCourier = Task.Run(() => GetDeliveryTypeAvailabilityDapper(inputData, Constants.CourierDelivery, token));
                taskYourTime = Task.Run(() => GetDeliveryTypeAvailabilityDapper(inputData, Constants.YourTimeDelivery, token));
            } else
            {
                taskSelf = Task.Run(() => GetDeliveryTypeAvailability(inputData, Constants.Self, token));
                taskCourier = Task.Run(() => GetDeliveryTypeAvailability(inputData, Constants.CourierDelivery, token));
                taskYourTime = Task.Run(() => GetDeliveryTypeAvailability(inputData, Constants.YourTimeDelivery, token));
            }

            // ожидаем завершения всех задач
            var results = await Task.WhenAll(taskSelf, taskCourier, taskYourTime);
            
            foreach (var taskResult in results)
            {
                var deliveryType = taskResult.deliveryType;

                if (deliveryType == Constants.Self) { result.Self.IsAvailable = taskResult.available; }
                if (deliveryType == Constants.CourierDelivery) { result.Courier.IsAvailable = taskResult.available; }
                if (deliveryType == Constants.YourTimeDelivery) { result.YourTime.IsAvailable = taskResult.available; }
            }

            _contextAccessor.HttpContext.Items["DatabaseConnection"] = string.Join(",", results.Select(obj => obj.connection));
            _contextAccessor.HttpContext.Items["LoadBalancingExecution"] = Convert.ToInt64(results.Max(obj => obj.loadBalancingTime ));
            _contextAccessor.HttpContext.Items["TimeSqlExecutionFact"] = Convert.ToInt64(results.Max(obj => obj.sqlExecutionTime));

            return result;
        }

        public async Task<ResponseIntervalList> GetIntervalList(RequestIntervalList inputData, CancellationToken token = default)
        {
            DbConnection dbConnection = await GetDbConnection(token: token);

            SqlConnection connection = dbConnection.Connection;
            connection.StatisticsEnabled = true;

            bool adressExists;
            string zoneId;

            Stopwatch watch = Stopwatch.StartNew();

            (adressExists, zoneId) = await _geoZones.CheckAddressGeozone(inputData, connection);

            watch.Stop();
            _contextAccessor.HttpContext.Items["TimeLocationExecution"] = watch.ElapsedMilliseconds;

            if (!adressExists && zoneId == "")
            {
                throw new ValidationException("Адрес и геозона не найдены!");
            }

            ResponseIntervalList result;

            if (_configuration.GetValue<bool>("UseDapper"))
            {
                result = await GetIntervalListDapper(inputData, connection, dbConnection.DatabaseType, zoneId, token);
            }
            else
            {
                result = await GetIntervalListBasic(inputData, dbConnection.DatabaseType, connection, zoneId, token);
            }

            return result;
        }

        public async Task<ResponseAvailableDateDictBothDates> GetAvailableDates(RequestDataAvailableDate inputData, CancellationToken token = default)
        {
            inputData.CheckQuantity = inputData.Codes.Any(x => x.Quantity != 1);

            ResponseAvailableDateDictBothDates result = new();

            if (!inputData.CheckQuantity)
            {
                var dataFromCache = await GetFromCache(inputData);

                foreach (var item in dataFromCache.Data)
                {
                    result.Data.Add(item.Key, item.Value);
                }

                if (result.Data.Count == inputData.Codes.Count)
                { 
                    return result; 
                }

                DeleteCachedDataFromInputData(inputData, dataFromCache);
            }

            var newDates = await GetAvailableDatesBasic(inputData, token);

            foreach (var item in newDates.Data)
            {
                result.Data.Add(item.Key, item.Value);
            }

            await SaveToCache(newDates, inputData.CityId);

            return result;
        }

        public async Task<ResponseAvailableDateDictBothDates> GetAvailableDatesBasic(RequestDataAvailableDate inputData, CancellationToken token = default)
        {
            DbConnection dbConnection = await GetDbConnection(token: token);

            SqlConnection connection = dbConnection.Connection;
            connection.StatisticsEnabled = true;

            ResponseAvailableDate dbResult = new();

            SqlCommand command = await AvailableDatesCommand(connection, inputData, dbConnection.DatabaseType, dbConnection.UseAggregations);

            Stopwatch watch = Stopwatch.StartNew();

            try
            { 
                //execute the SQLCommand
                SqlDataReader dr = await command.ExecuteReaderAsync(token);

                //check if there are records
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        var article = dr.GetString(0);
                        var code = dr.GetString(1);
                        var availableDateCourier = dr.GetDateTime(2).AddMonths(-24000);
                        var availableDateSelf = dr.GetDateTime(3).AddMonths(-24000);

                        dbResult.Article.Add(article);
                        dbResult.Code.Add(code);
                        dbResult.Courier.Add(new(availableDateCourier));
                        dbResult.Self.Add(new(availableDateSelf));
                    }
                }

                //close data reader
                _ = dr.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }

            watch.Stop();
            _contextAccessor.HttpContext.Items["TimeSqlExecutionFact"] = watch.ElapsedMilliseconds;

            var resultDict = new ResponseAvailableDateDict();

            try
            {
                foreach (var codeItem in inputData.Codes)
                {
                    var resultElement = new ResponseAvailableDateDictElement
                    {
                        Code = codeItem.Article,
                        SalesCode = codeItem.SalesCode,
                        Courier = null,
                        Self = null
                    };

                    int dbResultIndex = -1;
                    if (String.IsNullOrEmpty(codeItem.Code))
                    {
                        dbResultIndex = dbResult.Article.FindIndex(s => s == codeItem.Article);
                    }
                    else
                    {
                        dbResultIndex = dbResult.Code.FindIndex(s => s == codeItem.Code);
                    }
                    if (dbResultIndex == -1)
                        continue;

                    resultElement.Courier = inputData.DeliveryTypes.Contains("courier") && dbResult.Courier[dbResultIndex].Year != 3999
                        ? dbResult.Courier[dbResultIndex].Date.ToString("yyyy-MM-ddTHH:mm:ss")
                        : null;
                    resultElement.Self = inputData.DeliveryTypes.Contains("self") && dbResult.Self[dbResultIndex].Year != 3999
                        ? dbResult.Self[dbResultIndex].Date.ToString("yyyy-MM-ddTHH:mm:ss")
                        : null;

                    if (String.IsNullOrEmpty(codeItem.Code))
                    {
                        resultDict.Data.Add(codeItem.Article, resultElement);
                    }
                    else
                    {
                        resultDict.Data.Add(String.Concat(codeItem.Article, "_", codeItem.SalesCode), resultElement);
                    }
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Duplicated keys in dictionary", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }

            var resultBoth = new ResponseAvailableDateDictBothDates();

            foreach (var item in resultDict.Data)
            {
                var newItem = new ResponseAvailableDateDictElementBothDates
                {
                    Code = item.Value.Code,
                    SalesCode = item.Value.SalesCode,
                    Courier = item.Value.Courier,
                    Self = item.Value.Self
                };

                resultBoth.Data.Add(item.Key, newItem);
            }

            return resultBoth;
        }

        private async Task<ResponseIntervalList> GetIntervalListBasic(RequestIntervalList inputData, DatabaseType databaseType, SqlConnection connection, string zoneId, CancellationToken token = default)
        {
            var result = new ResponseIntervalList();

            SqlCommand command = await IntervalListCommand(connection, inputData, databaseType, zoneId);

            Stopwatch watch = Stopwatch.StartNew();

            try
            {
                SqlDataReader dr = command.ExecuteReader();

                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        var begin = dr.GetDateTime(0).AddMonths(-24000);
                        var end = dr.GetDateTime(1).AddMonths(-24000);
                        var bonus = dr.GetInt32(3) == 1;

                        result.Data.Add(new ResponseIntervalListElement
                        {
                            Begin = begin,
                            End = end,
                            Bonus = bonus
                        });
                    }
                }

                dr.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }

            watch.Stop();
            _contextAccessor.HttpContext.Items["TimeSqlExecutionFact"] = watch.ElapsedMilliseconds;

            return result;  
        }

        private async Task<ResponseIntervalList> GetIntervalListDapper(RequestIntervalList inputData, SqlConnection connection, DatabaseType databaseType, string zoneId, CancellationToken token = default)
        {
            var result = new ResponseIntervalList();

            var parameters1C = await GetGlobalParameters(connection, token);

            string query;
            DynamicParameters parameters;

            (query, parameters) = IntervalListCommand(inputData, parameters1C, databaseType, zoneId);

            Stopwatch watch = Stopwatch.StartNew();

            try
            {
                var results = await connection.QueryAsync<IntervalListQueryResult>(
                    new CommandDefinition(query, parameters, cancellationToken: token)
                );

                foreach(var element in results)
                {
                    var begin = element.StartDate.AddMonths(-24000);
                    var end = element.EndDate.AddMonths(-24000);
                    var bonus = element.Bonus == 1;

                    result.Data.Add(new ResponseIntervalListElement
                    {
                        Begin = begin,
                        End = end,
                        Bonus = bonus
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }

            watch.Stop();
            _contextAccessor.HttpContext.Items["TimeSqlExecutionFact"] = watch.ElapsedMilliseconds;

            return result;
        }

        private async Task<DeliveryTypeAvailabilityResult> GetDeliveryTypeAvailability(RequestAvailableDeliveryTypes inputData, string deliveryType, CancellationToken token = default)
        {
            DbConnection dbConnection = await GetDbConnection(false, token);

            using SqlConnection connection = dbConnection.Connection;
            connection.StatisticsEnabled = true;

            SqlCommand command = await AvailableDeliveryTypesCommand(connection, inputData, deliveryType, dbConnection.DatabaseType);

            bool deliveryTypeAvailable;

            var watch = Stopwatch.StartNew();

            try
            {
                object result = await command.ExecuteScalarAsync(token);

                deliveryTypeAvailable = result != null && (int)result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }

            watch.Stop();
            
            return new DeliveryTypeAvailabilityResult
            {
                deliveryType = deliveryType,
                available = deliveryTypeAvailable,
                loadBalancingTime = dbConnection.ConnectTimeInMilliseconds,
                sqlExecutionTime = watch.ElapsedMilliseconds,
                connection = dbConnection.ConnectionWithoutCredentials
            };
        }

        private async Task<DeliveryTypeAvailabilityResult> GetDeliveryTypeAvailabilityDapper(RequestAvailableDeliveryTypes inputData, string deliveryType, CancellationToken token = default)
        {
            DbConnection dbConnection = await GetDbConnection(false, token);

            using SqlConnection connection = dbConnection.Connection;
            connection.StatisticsEnabled = true;

            var parameters1C = await GetGlobalParameters(connection, token);
            
            string query;
            DynamicParameters parameters;

            (query, parameters ) = AvailableDeliveryTypesCommand(inputData, parameters1C, deliveryType, dbConnection.DatabaseType);

            bool deliveryTypeAvailable;

            var watch = Stopwatch.StartNew();

            try
            {
                var result = await connection.QueryAsync<int>(new CommandDefinition(query, parameters, cancellationToken: token));

                deliveryTypeAvailable = result != null && result.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }

            watch.Stop();

            return new DeliveryTypeAvailabilityResult
            {
                deliveryType = deliveryType,
                available = deliveryTypeAvailable,
                loadBalancingTime = dbConnection.ConnectTimeInMilliseconds,
                sqlExecutionTime = watch.ElapsedMilliseconds,
                connection = dbConnection.ConnectionWithoutCredentials
            };
        }

        private async Task<DbConnection> GetDbConnection(bool logging = true, CancellationToken token = default)
        {
            DbConnection dbConnection;

            try
            {
                dbConnection = await _loadBalancing.GetDatabaseConnectionAsync(token);
                if (logging)
                {
                    _contextAccessor.HttpContext.Items["DatabaseConnection"] = dbConnection.ConnectionWithoutCredentials;
                    _contextAccessor.HttpContext.Items["LoadBalancingExecution"] = dbConnection.ConnectTimeInMilliseconds;
                }
            }
            catch (Exception ex)
            {
                throw new DbConnectionNotFoundException(ex.Message);
            }

            if (dbConnection.Connection == null)
            {
                throw new DbConnectionNotFoundException("Не найдено доступное соединение к БД");
            }

            return dbConnection;
        }

        private async Task<SqlCommand> AvailableDeliveryTypesCommand(SqlConnection connection, RequestAvailableDeliveryTypes inputData, string deliveryType, DatabaseType databaseType)
        {
            var parameters1C = await GetGlobalParameters(connection);
            
            string query = AvailableDeliveryTypesQueries.AvailableDelivery;
            SqlCommand cmd = new(query, connection)
            {
                CommandTimeout = 5
            };

            var queryTextBegin = TextFillGoodsTable(inputData, cmd);

            if (_configuration.GetValue<bool>("disableKeepFixedPlan"))
            {
                query = query.Replace(", KEEPFIXED PLAN", "");
                queryTextBegin = queryTextBegin.Replace(", KEEPFIXED PLAN", "");
            }

            var DateMove = DateTime.Now.AddMonths(24000);

            cmd.Parameters.AddWithValue("@P_CityCode", inputData.CityId);
            cmd.Parameters.AddWithValue("@P_Floor", (double)(parameters1C.GetValue("Логистика_ЭтажПоУмолчанию")));
            cmd.Parameters.AddWithValue("@P_DaysToShow", 7);
            cmd.Parameters.AddWithValue("@P_DateTimeNow", DateMove);
            cmd.Parameters.AddWithValue("@P_DateTimePeriodBegin", DateMove.Date);
            cmd.Parameters.AddWithValue("@P_DateTimePeriodEnd", DateMove.Date.AddDays(parameters1C.GetValue("rsp_КоличествоДнейЗаполненияГрафика") - 1));
            cmd.Parameters.AddWithValue("@P_TimeNow", new DateTime(2001, 1, 1, DateMove.Hour, DateMove.Minute, DateMove.Second));
            cmd.Parameters.AddWithValue("@P_EmptyDate", new DateTime(2001, 1, 1, 0, 0, 0));
            cmd.Parameters.AddWithValue("@P_MaxDate", new DateTime(5999, 11, 11, 0, 0, 0));
            cmd.Parameters.AddWithValue("@P_ApplyShifting", (int)parameters1C.GetValue("ПрименятьСмещениеДоступностиПрослеживаемыхМаркируемыхТоваров"));
            cmd.Parameters.AddWithValue("@P_DaysToShift", (int)parameters1C.GetValue("КоличествоДнейСмещенияДоступностиПрослеживаемыхМаркируемыхТоваров"));
            cmd.Parameters.AddWithValue("@P_StockPriority", (int)parameters1C.GetValue("ПриоритизироватьСток_64854"));
            cmd.Parameters.AddWithValue("@P_YourTimeDelivery", deliveryType == Constants.YourTimeDelivery ? 1 : 0);
            cmd.Parameters.AddWithValue("@P_IsDelivery", deliveryType == Constants.Self ? 0 : 1);
            cmd.Parameters.AddWithValue("@P_IsPickup", deliveryType == Constants.Self ? 1 : 0);

            string pickupPointsString = string.Join(", ", inputData.PickupPoints
                .Select((value, index) =>
                {
                    string parameterName = $"@PickupPoint{index}";
                    cmd.Parameters.AddWithValue(parameterName, value);
                    return parameterName;
                }));

            string dateTimeNowOptimizeString = _configuration.GetValue<bool>("optimizeDateTimeNowEveryHour")
                ? DateMove.ToString("yyyy-MM-ddTHH:00:00")
                : DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss");

            cmd.CommandText = queryTextBegin + string.Format(query,
                dateTimeNowOptimizeString,
                DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                DateMove.Date.AddDays(parameters1C.GetValue("rsp_КоличествоДнейЗаполненияГрафика") - 1).ToString("yyyy-MM-ddTHH:mm:ss"),
                parameters1C.GetValue("КоличествоДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа"),
                parameters1C.GetValue("ПроцентДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа"),
                databaseType == DatabaseType.ReplicaTables ? _configuration.GetValue<string>("useIndexHintWarehouseDates") : "", // index hint
                pickupPointsString);

            return cmd;
        }

        private (string, DynamicParameters) AvailableDeliveryTypesCommand(RequestAvailableDeliveryTypes inputData, List<GlobalParam1C> parameters1C, string deliveryType, DatabaseType databaseType)
        {
            string query = AvailableDeliveryTypesQueries.AvailableDelivery;
            var parameters = new DynamicParameters();

            var queryTextBegin = TextFillGoodsTable(inputData, parameters);

            if (_configuration.GetValue<bool>("disableKeepFixedPlan"))
            {
                query = query.Replace(", KEEPFIXED PLAN", "");
                queryTextBegin = queryTextBegin.Replace(", KEEPFIXED PLAN", "");
            }

            var DateMove = DateTime.Now.AddMonths(24000);

            parameters.Add("@P_CityCode", inputData.CityId);
            parameters.Add("@P_Floor", (double)(parameters1C.GetValue("Логистика_ЭтажПоУмолчанию")));
            parameters.Add("@P_DaysToShow", 7);
            parameters.Add("@P_DateTimeNow", DateMove);
            parameters.Add("@P_DateTimePeriodBegin", DateMove.Date);
            parameters.Add("@P_DateTimePeriodEnd", DateMove.Date.AddDays(7 - 1));
            parameters.Add("@P_TimeNow", new DateTime(2001, 1, 1, DateMove.Hour, DateMove.Minute, DateMove.Second));
            parameters.Add("@P_EmptyDate", new DateTime(2001, 1, 1, 0, 0, 0));
            parameters.Add("@P_MaxDate", new DateTime(5999, 11, 11, 0, 0, 0));
            parameters.Add("@P_ApplyShifting", (int)parameters1C.GetValue("ПрименятьСмещениеДоступностиПрослеживаемыхМаркируемыхТоваров"));
            parameters.Add("@P_DaysToShift", (int)parameters1C.GetValue("КоличествоДнейСмещенияДоступностиПрослеживаемыхМаркируемыхТоваров"));
            parameters.Add("@P_StockPriority", (int)parameters1C.GetValue("ПриоритизироватьСток_64854"));
            parameters.Add("@P_YourTimeDelivery", deliveryType == Constants.YourTimeDelivery ? 1 : 0);
            parameters.Add("@P_IsDelivery", deliveryType == Constants.Self ? 0 : 1);
            parameters.Add("@P_IsPickup", deliveryType == Constants.Self ? 1 : 0);

            string pickupPointsString = string.Join(", ", inputData.PickupPoints
                .Select((value, index) =>
                {
                    string parameterName = $"@PickupPoint{index}";
                    parameters.Add(parameterName, value);
                    return parameterName;
                }));

            string dateTimeNowOptimizeString = _configuration.GetValue<bool>("optimizeDateTimeNowEveryHour")
                ? DateMove.ToString("yyyy-MM-ddTHH:00:00")
                : DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss");

            query = queryTextBegin + string.Format(query,
                dateTimeNowOptimizeString,
                DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                DateMove.Date.AddDays(7 - 1).ToString("yyyy-MM-ddTHH:mm:ss"),
                parameters1C.GetValue("КоличествоДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа"),
                parameters1C.GetValue("ПроцентДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа"),
                databaseType == DatabaseType.ReplicaTables ? _configuration.GetValue<string>("useIndexHintWarehouseDates") : "", // index hint
                pickupPointsString);

            return (query, parameters);
        }

        private async Task<SqlCommand> IntervalListCommand(SqlConnection connection, RequestIntervalList inputData, DatabaseType databaseType, string zoneId)
        {
            var parameters1C = await GetGlobalParameters(connection);
            
            string query = Queries.IntervalList;
            SqlCommand cmd = new(query, connection)
            {
                CommandTimeout = 5
            };

            var queryTextBegin = TextFillGoodsTable(inputData, cmd);

            if (_configuration.GetValue<bool>("disableKeepFixedPlan"))
            {
                query = query.Replace(", KEEPFIXED PLAN", "");
                queryTextBegin = queryTextBegin.Replace(", KEEPFIXED PLAN", "");
            }

            var yourTimeDelivery = false;

            if (inputData.DeliveryType == Constants.YourTimeDelivery)
            {
                inputData.DeliveryType = Constants.CourierDelivery;
                yourTimeDelivery = true;
            }

            var DateMove = DateTime.Now.AddMonths(24000);

            cmd.Parameters.AddWithValue("@P_AdressCode", inputData.AddressId != null ? inputData.AddressId : DBNull.Value);
            cmd.Parameters.AddWithValue("@PickupPoint1", inputData.PickupPoint != null ? inputData.PickupPoint : DBNull.Value);
            cmd.Parameters.AddWithValue("@P_Credit", inputData.Payment == "partly_pay" ? 1 : 0);
            cmd.Parameters.AddWithValue("@P_Floor", (double)(inputData.Floor != null ? inputData.Floor : parameters1C.GetValue("Логистика_ЭтажПоУмолчанию")));
            cmd.Parameters.AddWithValue("@P_DaysToShow", 7);
            cmd.Parameters.AddWithValue("@P_DateTimeNow", DateMove);
            cmd.Parameters.AddWithValue("@P_DateTimePeriodBegin", DateMove.Date);
            cmd.Parameters.AddWithValue("@P_DateTimePeriodEnd", DateMove.Date.AddDays(parameters1C.GetValue("rsp_КоличествоДнейЗаполненияГрафика") - 1));
            cmd.Parameters.AddWithValue("@P_TimeNow", new DateTime(2001, 1, 1, DateMove.Hour, DateMove.Minute, DateMove.Second));
            cmd.Parameters.AddWithValue("@P_EmptyDate", new DateTime(2001, 1, 1, 0, 0, 0));
            cmd.Parameters.AddWithValue("@P_MaxDate", new DateTime(5999, 11, 11, 0, 0, 0));
            cmd.Parameters.AddWithValue("@P_GeoCode", zoneId);
            cmd.Parameters.AddWithValue("@P_OrderDate", inputData.OrderDate.AddMonths(24000));
            cmd.Parameters.AddWithValue("@P_OrderNumber", inputData.OrderNumber != null ? inputData.OrderNumber : DBNull.Value);
            cmd.Parameters.AddWithValue("@P_ApplyShifting", (int)parameters1C.GetValue("ПрименятьСмещениеДоступностиПрослеживаемыхМаркируемыхТоваров"));
            cmd.Parameters.AddWithValue("@P_DaysToShift", (int)parameters1C.GetValue("КоличествоДнейСмещенияДоступностиПрослеживаемыхМаркируемыхТоваров"));
            cmd.Parameters.AddWithValue("@P_StockPriority", (int)parameters1C.GetValue("ПриоритизироватьСток_64854"));
            cmd.Parameters.AddWithValue("@P_YourTimeDelivery", yourTimeDelivery ? 1 : 0);

            string dateTimeNowOptimizeString = _configuration.GetValue<bool>("optimizeDateTimeNowEveryHour")
                ? DateMove.ToString("yyyy-MM-ddTHH:00:00")
                : DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss");

            cmd.CommandText = queryTextBegin + string.Format(query,
                "",
                dateTimeNowOptimizeString,
                DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                DateMove.Date.AddDays(parameters1C.GetValue("rsp_КоличествоДнейЗаполненияГрафика") - 1).ToString("yyyy-MM-ddTHH:mm:ss"),
                parameters1C.GetValue("КоличествоДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа"),
                parameters1C.GetValue("ПроцентДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа"),
                databaseType == DatabaseType.ReplicaTables ? _configuration.GetValue<string>("useIndexHintWarehouseDates") : ""); // index hint

            return cmd;
        }

        private (string, DynamicParameters) IntervalListCommand(RequestIntervalList inputData, List<GlobalParam1C> parameters1C, DatabaseType databaseType, string zoneId)
        {
            string query = Queries.IntervalList;
            var parameters = new DynamicParameters(); 

            var queryTextBegin = TextFillGoodsTable(inputData, parameters);

            if (_configuration.GetValue<bool>("disableKeepFixedPlan"))
            {
                query = query.Replace(", KEEPFIXED PLAN", "");
                queryTextBegin = queryTextBegin.Replace(", KEEPFIXED PLAN", "");
            }

            var yourTimeDelivery = false;

            if (inputData.DeliveryType == Constants.YourTimeDelivery)
            {
                inputData.DeliveryType = Constants.CourierDelivery;
                yourTimeDelivery = true;
            }

            var DateMove = DateTime.Now.AddMonths(24000);

            parameters.Add("@P_AdressCode", inputData.AddressId);
            parameters.Add("@PickupPoint1", inputData.PickupPoint);
            parameters.Add("@P_Credit", inputData.Payment == "partly_pay" ? 1 : 0);
            parameters.Add("@P_Floor", (double)(inputData.Floor != null ? inputData.Floor : parameters1C.GetValue("Логистика_ЭтажПоУмолчанию")));
            parameters.Add("@P_DaysToShow", 7);
            parameters.Add("@P_DateTimeNow", DateMove);
            parameters.Add("@P_DateTimePeriodBegin", DateMove.Date);
            parameters.Add("@P_DateTimePeriodEnd", DateMove.Date.AddDays(parameters1C.GetValue("rsp_КоличествоДнейЗаполненияГрафика") - 1));
            parameters.Add("@P_TimeNow", new DateTime(2001, 1, 1, DateMove.Hour, DateMove.Minute, DateMove.Second));
            parameters.Add("@P_EmptyDate", new DateTime(2001, 1, 1, 0, 0, 0));
            parameters.Add("@P_MaxDate", new DateTime(5999, 11, 11, 0, 0, 0));
            parameters.Add("@P_GeoCode", zoneId);
            parameters.Add("@P_OrderDate", inputData.OrderDate.AddMonths(24000));
            parameters.Add("@P_OrderNumber", inputData.OrderNumber);
            parameters.Add("@P_ApplyShifting", (int)parameters1C.GetValue("ПрименятьСмещениеДоступностиПрослеживаемыхМаркируемыхТоваров"));
            parameters.Add("@P_DaysToShift", (int)parameters1C.GetValue("КоличествоДнейСмещенияДоступностиПрослеживаемыхМаркируемыхТоваров"));
            parameters.Add("@P_StockPriority", (int)parameters1C.GetValue("ПриоритизироватьСток_64854"));
            parameters.Add("@P_YourTimeDelivery", yourTimeDelivery ? 1 : 0);

            string dateTimeNowOptimizeString = _configuration.GetValue<bool>("optimizeDateTimeNowEveryHour")
                ? DateMove.ToString("yyyy-MM-ddTHH:00:00")
                : DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss");

            query = queryTextBegin + string.Format(query,
                "",
                dateTimeNowOptimizeString,
                DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                DateMove.Date.AddDays(parameters1C.GetValue("rsp_КоличествоДнейЗаполненияГрафика") - 1).ToString("yyyy-MM-ddTHH:mm:ss"),
                parameters1C.GetValue("КоличествоДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа"),
                parameters1C.GetValue("ПроцентДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа"),
                databaseType == DatabaseType.ReplicaTables ? _configuration.GetValue<string>("useIndexHintWarehouseDates") : ""); // index hint

            return (query, parameters);
        }

        private async Task<SqlCommand> AvailableDatesCommand(SqlConnection connection, RequestDataAvailableDate inputData, DatabaseType databaseType, bool customAggs)
        {
            var parameters1C = await GetGlobalParameters(connection);

            string query = "";

            SqlCommand cmd = new(query, connection)
            {
                CommandTimeout = 5
            };

            List<string> pickups = new();

            var queryTextBegin = TextFillGoodsTable(inputData, cmd, true, pickups);

            if (_configuration.GetValue<bool>("disableKeepFixedPlan"))
            {
                queryTextBegin = queryTextBegin.Replace(", KEEPFIXED PLAN", "");
            }
            List<string> queryParts = new()
            {
                inputData.CheckQuantity == true ? Queries.AvailableDateWithCount1 : Queries.AvailableDate1,
                customAggs == true ? Queries.AvailableDate2MinimumWarehousesCustom : Queries.AvailableDate2MinimumWarehousesBasic,
                inputData.CheckQuantity == true ? Queries.AvailableDateWithCount3 : Queries.AvailableDate3,
                Queries.AvailableDate4SourcesWithPrices,
                inputData.CheckQuantity == true ? Queries.AvailableDateWithCount5 : Queries.AvailableDate5,
                customAggs == true ? Queries.AvailableDate6IntervalsCustom : Queries.AvailableDate6IntervalsBasic,
                Queries.AvailableDate7,
                customAggs == true ? Queries.AvailableDate8DeliveryPowerCustom : Queries.AvailableDate8DeliveryPowerBasic,
                Queries.AvailableDate9
            };

            query = String.Join("", queryParts);

            List<string> pickupParameters = new();
            foreach (var pickupPoint in pickups)
            {
                var parameterString = string.Format("@PickupPointAll{0}", pickups.IndexOf(pickupPoint));
                pickupParameters.Add(parameterString);
                cmd.Parameters.Add(parameterString, SqlDbType.NVarChar, 4);
                cmd.Parameters[parameterString].Value = pickupPoint;
            }
            if (pickupParameters.Count == 0)
            {
                pickupParameters.Add("NULL");
            }

            var DateMove = DateTime.Now.AddMonths(24000);
            
            cmd.Parameters.AddWithValue("@P_CityCode", inputData.CityId);          
            cmd.Parameters.AddWithValue("@P_DateTimeNow", DateMove);
            cmd.Parameters.AddWithValue("@P_DateTimePeriodBegin", DateMove.Date);         
            cmd.Parameters.AddWithValue("@P_DateTimePeriodEnd", DateMove.Date.AddDays(parameters1C.GetValue("rsp_КоличествоДнейЗаполненияГрафика") - 1));
            cmd.Parameters.AddWithValue("@P_TimeNow", new DateTime(2001, 1, 1, DateMove.Hour, DateMove.Minute, DateMove.Second));
            cmd.Parameters.AddWithValue("@P_EmptyDate", new DateTime(2001, 1, 1, 0, 0, 0));
            cmd.Parameters.AddWithValue("@P_MaxDate", new DateTime(5999, 11, 11, 0, 0, 0));
            cmd.Parameters.AddWithValue("@P_DaysToShow", 7);
            cmd.Parameters.AddWithValue("@P_ApplyShifting", (int)parameters1C.GetValue("ПрименятьСмещениеДоступностиПрослеживаемыхМаркируемыхТоваров"));
            cmd.Parameters.AddWithValue("@P_DaysToShift", (int)parameters1C.GetValue("КоличествоДнейСмещенияДоступностиПрослеживаемыхМаркируемыхТоваров"));

            if (inputData.CheckQuantity)
            {
                cmd.Parameters.AddWithValue("@P_StockPriority", (int)parameters1C.GetValue("ПриоритизироватьСток_64854"));
            }

            string dateTimeNowOptimizeString = _configuration.GetValue<bool>("optimizeDateTimeNowEveryHour")
                ? DateMove.ToString("yyyy-MM-ddTHH:00:00")
                : DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss");

            var pickupWorkingHoursJoinType = _configuration.GetValue<string>("pickupWorkingHoursJoinType");

            string useIndexHint = _configuration.GetValue<string>("useIndexHintWarehouseDates");// @", INDEX([_InfoRg23830_Custom2])";
            if (databaseType != DatabaseType.ReplicaTables || customAggs)
            {
                useIndexHint = "";
            }

            cmd.CommandText = queryTextBegin + string.Format(query, string.Join(",", pickupParameters),
                dateTimeNowOptimizeString,
                DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                DateMove.Date.AddDays(parameters1C.GetValue("rsp_КоличествоДнейЗаполненияГрафика") - 1).ToString("yyyy-MM-ddTHH:mm:ss"),
                parameters1C.GetValue("КоличествоДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа"),
                parameters1C.GetValue("ПроцентДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа"),
                pickupWorkingHoursJoinType,
                useIndexHint);

            return cmd;
        }

        private static string TextFillGoodsTable(RequestAvailableDeliveryTypes data, SqlCommand cmdGoodsTable)
        {
            var resultString = AvailableDeliveryTypesQueries.GoodsRawCreate;

            var parameters = data.OrderItems.Select((item, index) =>
            {
                var article = $"@Article{index}";
                var code = $"@Code{index}";
                var quantity = $"@Quantity{index}";

                cmdGoodsTable.Parameters.AddWithValue(article, item.Article);
                cmdGoodsTable.Parameters.AddWithValue(code, string.IsNullOrEmpty(item.Code) ? DBNull.Value : item.Code);
                cmdGoodsTable.Parameters.AddWithValue(quantity, item.Quantity);

                return $"({article}, {code}, {quantity})";
            }).ToList();

            if (parameters.Count > 0)
            {
                resultString += string.Format(AvailableDeliveryTypesQueries.GoodsRawInsert, string.Join(", ", parameters));
            }

            return resultString;
        }

        private static string TextFillGoodsTable(RequestAvailableDeliveryTypes data, DynamicParameters dynamicParameters)
        {
            var resultString = AvailableDeliveryTypesQueries.GoodsRawCreate;

            var parameters = data.OrderItems.Select((item, index) =>
            {
                var article = $"@Article{index}";
                var code = $"@Code{index}";
                var quantity = $"@Quantity{index}";

                dynamicParameters.Add(article, item.Article);
                dynamicParameters.Add(code, string.IsNullOrEmpty(item.Code) ? null : item.Code);
                dynamicParameters.Add(quantity, item.Quantity);

                return $"({article}, {code}, {quantity})";
            }).ToList();

            if (parameters.Count > 0)
            {
                resultString += string.Format(AvailableDeliveryTypesQueries.GoodsRawInsert, string.Join(", ", parameters));
            }

            return resultString;
        }

        private static string TextFillGoodsTable(RequestIntervalList data, SqlCommand cmdGoodsTable)
        {
            var resultString = Queries.CreateTableGoodsRawCreate;

            var parameters = data.OrderItems.Select((item, index) =>
            {
                var article = $"@Article{index}";
                var code = $"@Code{index}";
                var quantity = $"@Quantity{index}";

                cmdGoodsTable.Parameters.AddWithValue(article, item.Article);
                cmdGoodsTable.Parameters.AddWithValue(code, string.IsNullOrEmpty(item.Code) ? DBNull.Value : item.Code);
                cmdGoodsTable.Parameters.AddWithValue(quantity, item.Quantity);

                return $"({article}, {code}, NULL, {quantity})";
            }).ToList();

            if (parameters.Count > 0)
            {
                resultString += string.Format(Queries.CreateTableGoodsRawInsert, string.Join(", ", parameters));
            }

            return resultString;
        }

        private static string TextFillGoodsTable(RequestIntervalList data, DynamicParameters dynamicParameters)
        {
            var resultString = Queries.CreateTableGoodsRawCreate;

            var parameters = data.OrderItems.Select((item, index) =>
            {
                var article = $"@Article{index}";
                var code = $"@Code{index}";
                var quantity = $"@Quantity{index}";

                dynamicParameters.Add(article, item.Article);
                dynamicParameters.Add(code, string.IsNullOrEmpty(item.Code) ? null : item.Code);
                dynamicParameters.Add(quantity, item.Quantity);

                return $"({article}, {code}, NULL, {quantity})";
            }).ToList();

            if (parameters.Count > 0)
            {
                resultString += string.Format(Queries.CreateTableGoodsRawInsert, string.Join(", ", parameters));
            }

            return resultString;
        }

        public static string TextFillGoodsTable(RequestDataAvailableDate data, SqlCommand cmdGoodsTable, bool optimizeRowsCount, List<string> PickupsList)
        {

            var resultString = Queries.CreateTableGoodsRawCreate;

            var insertRowsLimit = 900;

            var parameters = new List<string>();

            data.Codes = data.Codes.Where(x =>
            {
                if (data.CheckQuantity)
                {
                    return x.Quantity > 0;
                }
                else return true;
            }).ToList();

            if (data.CheckQuantity)
            {
                data.CheckQuantity = data.Codes.Any(x => x.Quantity != 1); //we can use basic query if all quantity is 1
            }

            var maxCodes = data.Codes.Count;

            foreach (var codesElem in data.Codes)
            {
                foreach (var item in codesElem.PickupPoints)
                {
                    if (!PickupsList.Contains(item))
                    {
                        PickupsList.Add(item);
                    }
                }
            }

            int maxPickups = PickupsList.Count;

            if (data.Codes.Count > 2) maxCodes = 10;
            if (data.Codes.Count > 10) maxCodes = 30;
            if (data.Codes.Count > 30) maxCodes = 60;
            if (data.Codes.Count > 60) maxCodes = 100;
            if (data.Codes.Count > maxCodes || !optimizeRowsCount) maxCodes = data.Codes.Count;


            for (int codesCounter = 0; codesCounter < maxCodes; codesCounter++)
            {

                RequestDataCodeItem codesElem;
                if (codesCounter < data.Codes.Count)
                {
                    codesElem = data.Codes[codesCounter];
                }
                else
                {
                    codesElem = data.Codes[^1];
                }



                var parameterString = string.Format("(@Article{0}, @Code{0}, NULL, @Quantity{0})", codesCounter);


                //cmdGoodsTable.Parameters.AddWithValue(string.Format("@Article{0}", codesCounter), codesElem.article);
                cmdGoodsTable.Parameters.Add(string.Format("@Article{0}", codesCounter), SqlDbType.NVarChar, 11);
                cmdGoodsTable.Parameters[string.Format("@Article{0}", codesCounter)].Value = codesElem.Article;

                cmdGoodsTable.Parameters.Add(string.Format("@Code{0}", codesCounter), SqlDbType.NVarChar, 11);
                if (String.IsNullOrEmpty(codesElem.Code))
                    cmdGoodsTable.Parameters[string.Format("@Code{0}", codesCounter)].Value = DBNull.Value;
                else
                    cmdGoodsTable.Parameters[string.Format("@Code{0}", codesCounter)].Value = codesElem.Code;

                //cmdGoodsTable.Parameters.AddWithValue(string.Format("@Quantity{0}", codesCounter), codesElem.quantity);
                cmdGoodsTable.Parameters.Add(string.Format("@Quantity{0}", codesCounter), SqlDbType.Int, 10);
                cmdGoodsTable.Parameters[string.Format("@Quantity{0}", codesCounter)].Value = codesElem.Quantity;

                parameters.Add(parameterString);

                if (parameters.Count == insertRowsLimit)
                {
                    resultString += string.Format(Queries.CreateTableGoodsRawInsert, string.Join(", ", parameters));

                    parameters.Clear();
                }

                if (maxPickups > 0)
                {
                    var PickupParameter = string.Join(",", codesElem.PickupPoints);

                    cmdGoodsTable.Parameters.Add(string.Format("@PickupPoint{0}", codesCounter), SqlDbType.NVarChar, 45);
                    cmdGoodsTable.Parameters[string.Format("@PickupPoint{0}", codesCounter)].Value = PickupParameter;

                    var parameterStringPickup = string.Format("(@Article{0}, @Code{0}, @PickupPoint{0}, @Quantity{0})", codesCounter);
                    parameters.Add(parameterStringPickup);
                }

            }

            if (parameters.Count > 0)
            {
                resultString += string.Format(Queries.CreateTableGoodsRawInsert, string.Join(", ", parameters));

                parameters.Clear();
            }

            return resultString;

        }

        private async Task<List<GlobalParam1C>> GetGlobalParameters(SqlConnection connection, CancellationToken token = default)
        {
            string key = "GlobalParameters";
            // кешируем ГП в памяти на 1 час, потом они снова обновятся
            return await _memoryCache.GetOrCreateAsync<List<GlobalParam1C>>(
                key,
                async entry =>
                {
                    entry.SetAbsoluteExpiration(TimeSpan.FromHours(1));

                    var parameters = new List<GlobalParam1C>
                    {
                        new GlobalParam1C
                        {
                            Name = "rsp_КоличествоДнейЗаполненияГрафика",
                            DefaultDouble = 5
                        },
                        new GlobalParam1C
                        {
                            Name = "КоличествоДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа",
                            DefaultDouble = 4
                        },
                        new GlobalParam1C
                        {
                            Name = "ПроцентДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа",
                            DefaultDouble = 3
                        },
                        new GlobalParam1C
                        {
                            Name = "Логистика_ЭтажПоУмолчанию",
                            DefaultDouble = 4,
                            UseDefault = true
                        },
                        new GlobalParam1C
                        {
                            Name = "ПрименятьСмещениеДоступностиПрослеживаемыхМаркируемыхТоваров",
                            DefaultDouble = 0
                        },
                        new GlobalParam1C
                        {
                            Name = "КоличествоДнейСмещенияДоступностиПрослеживаемыхМаркируемыхТоваров",
                            DefaultDouble = 0
                        },
                        new GlobalParam1C
                        {
                            Name = "ПриоритизироватьСток_64854",
                            DefaultDouble = 0
                        }
                    };
                    await GlobalParam1C.FillValues(connection, parameters, _logger, token);
                    return parameters;
                });
        }

        private async Task SaveToCache(ResponseAvailableDateDictBothDates result, string cityId)
        {
            IDatabase database = _redis.GetDatabase();
            // Время жизни ключей
            var expiry = TimeSpan.FromSeconds(_redisSettings.LifeTime);

            // Запись пар ключ-значение в Redis
            foreach (var item in result.Data)
            {
                var key = $"{item.Key}-{cityId}";
                await database.StringSetAsync(key, JsonSerializer.Serialize(item.Value), expiry);
            }
        }

        private async Task<ResponseAvailableDateDictBothDates> GetFromCache(RequestDataAvailableDate inputData)
        {
            var result = new ResponseAvailableDateDictBothDates();

            IDatabase database = _redis.GetDatabase();

            foreach (var item in inputData.Codes)
            {
                var keyField = item.Code is not null ? item.Code : item.Article;
                var key = $"{keyField}-{inputData.CityId}";

                string value = await database.StringGetAsync(key);

                if (value is null) { continue; }

                var newItem = JsonSerializer.Deserialize<ResponseAvailableDateDictElementBothDates>(value);

                result.Data.Add(item.Article, newItem);
            }

            return result;
        }

        private static void DeleteCachedDataFromInputData(RequestDataAvailableDate inputData, ResponseAvailableDateDictBothDates dataFromCache)
        {
            for (int i = inputData.Codes.Count - 1; i >= 0; i--)
            {
                var item = inputData.Codes[i];
                var keyField = item.Code is not null ? item.Code : item.Article;
                
                if (dataFromCache.Data.ContainsKey(keyField))
                {
                    inputData.Codes.RemoveAt(i);
                }
            }
        }

        private class DeliveryTypeAvailabilityResult
        {
            public string deliveryType;
            public bool available;
            public long loadBalancingTime;
            public long sqlExecutionTime;
            public string connection;
        }
    
        private class IntervalListQueryResult
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int OrdersCount { get; set; }
            public int Bonus { get; set; }
        }
    }
}
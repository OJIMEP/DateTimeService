using DateTimeService.Controllers;
using DateTimeService.Data;
using DateTimeService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using DateTimeService.Exceptions;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace DateTimeService.Services
{
    public class IntervalListDataService: IDataService<RequestIntervalList, ResponseIntervalList>
    {
        private readonly IConfiguration _configuration;
        private readonly ILoadBalancing _loadBalancing;
        private readonly ILogger<DateTimeController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IGeoZones _geoZones;

        public IntervalListDataService(IConfiguration configuration, ILoadBalancing loadBalancing, ILogger<DateTimeController> logger, 
            IHttpContextAccessor contextAccessor, IGeoZones geoZones)
        {
            _configuration = configuration;
            _loadBalancing = loadBalancing;
            _logger = logger;
            _contextAccessor = contextAccessor;
            _geoZones = geoZones;
        }

        public async Task<ResponseIntervalList> GetDataByParam(RequestIntervalList inputData)
        {
            var result = await GetIntervalList(inputData);

            return result;
        }

        private async Task<ResponseIntervalList> GetIntervalList(RequestIntervalList inputData)
        {
            var result = new ResponseIntervalList();

            DbConnection dbConnection = await GetDbConnection();

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

            SqlCommand command = GetSqlCommand(connection, inputData, dbConnection.DatabaseType, zoneId);

            watch.Restart();

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

        private SqlCommand GetSqlCommand(SqlConnection connection, RequestIntervalList inputData, string databaseType, string zoneId)
        {
            var parameters1C = GetGlobalParameters();
            GlobalParam1C.FillValues(connection, parameters1C, _logger);

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
                databaseType == "replica_tables" ? _configuration.GetValue<string>("useIndexHintWarehouseDates") : ""); // index hint

            return cmd;
        }

        private async Task<DbConnection> GetDbConnection()
        {
            DbConnection dbConnection;

            var watch = Stopwatch.StartNew();

            try
            {
                dbConnection = await _loadBalancing.GetDatabaseConnectionAsync();
                watch.Stop();
                _contextAccessor.HttpContext.Items["DatabaseConnection"] = dbConnection.ConnectionWithoutCredentials;
                _contextAccessor.HttpContext.Items["LoadBalancingExecution"] = watch.ElapsedMilliseconds;
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
       
        private static List<GlobalParam1C> GetGlobalParameters()
        {
            return new List<GlobalParam1C>
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
        }
    }
}

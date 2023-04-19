using DateTimeService.Controllers;
using DateTimeService.Data;
using DateTimeService.Exceptions;
using DateTimeService.Models.AvailableDeliveryTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DateTimeService.Services
{
    public class DeliveryTypesDataService : IDataService<RequestAvailableDeliveryTypes, ResponseAvailableDeliveryTypes>
    {
        private readonly IConfiguration _configuration;
        private readonly ILoadBalancing _loadBalancing;
        private readonly ILogger<DateTimeController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;

        public DeliveryTypesDataService(IConfiguration configuration, ILoadBalancing loadBalancing, ILogger<DateTimeController> logger, IHttpContextAccessor contextAccessor)
        {
            _configuration = configuration;
            _loadBalancing = loadBalancing;
            _logger = logger;
            _contextAccessor = contextAccessor;
        }

        public async Task<ResponseAvailableDeliveryTypes> GetDataByParam(RequestAvailableDeliveryTypes inputData)
        {
            var result = new ResponseAvailableDeliveryTypes();

            Task<(string, bool)> taskSelf = Task.Run(() => GetDeliveryTypeAvailability(inputData, Constants.Self));
            Task<(string, bool)> taskCourier = Task.Run(() => GetDeliveryTypeAvailability(inputData, Constants.CourierDelivery));
            Task<(string, bool)> taskYourTime = Task.Run(() => GetDeliveryTypeAvailability(inputData, Constants.YourTimeDelivery));

            // ожидаем завершения всех задач
            var results = await Task.WhenAll(taskSelf, taskCourier, taskYourTime);

            foreach (var taskResult in results)
            {
                var deliveryType = taskResult.Item1;

                if (deliveryType == Constants.Self) { result.Self.IsAvailable = taskResult.Item2; }
                if (deliveryType == Constants.CourierDelivery) { result.Courier.IsAvailable = taskResult.Item2; }
                if (deliveryType == Constants.YourTimeDelivery) { result.YourTime.IsAvailable = taskResult.Item2; }
            }

            return result;
        }

        private async Task<(string, bool)> GetDeliveryTypeAvailability(RequestAvailableDeliveryTypes inputData, string deliveryType)
        {
            var watch = Stopwatch.StartNew();

            DbConnection dbConnection = await GetDbConnection();

            SqlConnection connection = dbConnection.Connection;
            connection.StatisticsEnabled = true;

            SqlCommand command = GetSqlCommand(connection, inputData, deliveryType, dbConnection.DatabaseType);

            bool deliveryTypeAvailable;
            try
            {
                object result = await command.ExecuteScalarAsync();

                deliveryTypeAvailable = result != null && (int)result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }

            watch.Stop();
            _contextAccessor.HttpContext.Items["TimeSqlExecutionFact"] = watch.ElapsedMilliseconds;

            return (deliveryType, deliveryTypeAvailable);
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

        private SqlCommand GetSqlCommand(SqlConnection connection, RequestAvailableDeliveryTypes inputData, string deliveryType, string databaseType)
        {
            var parameters1C = GetGlobalParameters();
            GlobalParam1C.FillValues(connection, parameters1C, _logger);

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
                databaseType == "replica_tables" ? _configuration.GetValue<string>("useIndexHintWarehouseDates") : "", // index hint
                pickupPointsString);

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

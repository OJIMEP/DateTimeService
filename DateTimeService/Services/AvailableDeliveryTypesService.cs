using DateTimeService.Models.AvailableDeliveryTypes;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using DateTimeService.Data;
using Microsoft.Extensions.Configuration;
using DateTimeService.Exceptions;
using System.Collections.Generic;
using DateTimeService.Controllers;
using Microsoft.Extensions.Logging;
using DateTimeService.Models;
using System.Data;
using System.Linq;
using DateTimeService.Logging;

namespace DateTimeService.Services
{
    public class AvailableDeliveryTypesService : IAvailableDeliveryTypesService
    {
        private readonly IConfiguration _configuration;
        private readonly ILoadBalancing _loadBalacing;
        private readonly ILogger<DateTimeController> _logger;
        private LogElementInternal logInternal;

        public AvailableDeliveryTypesService(IConfiguration configuration, ILoadBalancing loadBalancing, ILogger<DateTimeController> logger)
        {
            _configuration = configuration;
            _loadBalacing = loadBalancing;
            _logger = logger;
        }

        public async Task<ResponseAvailableDeliveryTypes> GetAvailableDeliveryTypes(RequestAvailableDeliveryTypes inputData)
        {
            var watch = Stopwatch.StartNew();

            var result = new ResponseAvailableDeliveryTypes();

            logInternal = new();

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

            watch.Stop();
            logInternal.TimeFullExecution = watch.ElapsedMilliseconds;

            return result;
        }

        public LogElementInternal GetLog()
        {
            return logInternal;
        }

        private async Task<(string, bool)> GetDeliveryTypeAvailability(RequestAvailableDeliveryTypes inputData, string deliveryType)
        {
            var deliveryTypeAvailable = false;

            var watch = Stopwatch.StartNew();

            DbConnection dbConnection = await GetDbConnection(deliveryType);
            SqlConnection connection = dbConnection.Connection;

            connection.StatisticsEnabled = true;

            try
            {
                SqlCommand command = GetDeliveryTypeAvailabilityCommand(connection, inputData, deliveryType, dbConnection.DatabaseType);

                object result = await command.ExecuteScalarAsync();

                deliveryTypeAvailable = result != null && (int)result > 0;
            }
            catch (Exception ex)
            {
                logInternal.AddError(deliveryType, ex.Message);
            }
            
            watch.Stop();
            logInternal.AddExecutionFact(watch.ElapsedMilliseconds);
            logInternal.AddStatistics(connection.RetrieveStatistics());

            return (deliveryType, deliveryTypeAvailable);
        }

        private async Task<DbConnection> GetDbConnection(string deliveryType)
        {
            DbConnection dbConnection;

            var watch = Stopwatch.StartNew();

            try
            {
                dbConnection = await _loadBalacing.GetDatabaseConnectionAsync();
                watch.Stop();
                logInternal.AddDatabaseConnection(dbConnection.ConnectionWithoutCredentials);
                logInternal.AddDbConnectTime(watch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                logInternal.AddError(deliveryType, ex.Message);
                throw new DbConnectionNotFoundException(ex.Message);
            }

            if (dbConnection.Connection == null)
            {
                logInternal.AddError(deliveryType, "Не найдено доступное соединение к БД");
                throw new DbConnectionNotFoundException("Не найдено доступное соединение к БД");
            }

            return dbConnection;
        }

        private SqlCommand GetDeliveryTypeAvailabilityCommand(SqlConnection connection, RequestAvailableDeliveryTypes inputData, string deliveryType, string databaseType)
        {
            var parameters1C = GetGlobalParameters();

            GlobalParam1C.FillValues(connection, parameters1C, _logger);

            string query = AvailableDeliveryTypesQueries.AvailableDelivery;
            SqlCommand cmd = new(query, connection);

            var queryTextBegin = TextFillGoodsTable(inputData, cmd);

            if (_configuration.GetValue<bool>("disableKeepFixedPlan"))
            {
                query = query.Replace(", KEEPFIXED PLAN", "");
                queryTextBegin = queryTextBegin.Replace(", KEEPFIXED PLAN", "");
            }

            var DateMove = DateTime.Now.AddMonths(24000);
            var TimeNow = new DateTime(2001, 1, 1, DateMove.Hour, DateMove.Minute, DateMove.Second);
            var EmptyDate = new DateTime(2001, 1, 1, 0, 0, 0);
            var MaxDate = new DateTime(5999, 11, 11, 0, 0, 0);

            cmd.Parameters.Add("@P_CityCode", SqlDbType.NVarChar, 20);
            cmd.Parameters["@P_CityCode"].Value = inputData.CityId;

            cmd.Parameters.Add("@P_Floor", SqlDbType.Float);
            cmd.Parameters["@P_Floor"].Value = (double)(parameters1C.First(x => x.Name.Contains("Логистика_ЭтажПоУмолчанию")).ValueDouble);

            cmd.Parameters.Add("@P_DaysToShow", SqlDbType.Int);
            cmd.Parameters["@P_DaysToShow"].Value = 7;

            cmd.Parameters.Add("@P_DateTimeNow", SqlDbType.DateTime);
            cmd.Parameters["@P_DateTimeNow"].Value = DateMove;

            cmd.Parameters.Add("@P_DateTimePeriodBegin", SqlDbType.DateTime);
            cmd.Parameters["@P_DateTimePeriodBegin"].Value = DateMove.Date;

            cmd.Parameters.Add("@P_DateTimePeriodEnd", SqlDbType.DateTime);
            cmd.Parameters["@P_DateTimePeriodEnd"].Value = DateMove.Date.AddDays(parameters1C.First(x => x.Name.Contains("rsp_КоличествоДнейЗаполненияГрафика")).ValueDouble - 1);

            cmd.Parameters.Add("@P_TimeNow", SqlDbType.DateTime);
            cmd.Parameters["@P_TimeNow"].Value = TimeNow;

            cmd.Parameters.Add("@P_EmptyDate", SqlDbType.DateTime);
            cmd.Parameters["@P_EmptyDate"].Value = EmptyDate;

            cmd.Parameters.Add("@P_MaxDate", SqlDbType.DateTime);
            cmd.Parameters["@P_MaxDate"].Value = MaxDate;

            cmd.Parameters.Add("@P_ApplyShifting", SqlDbType.Int);
            cmd.Parameters["@P_ApplyShifting"].Value = parameters1C.First(x => x.Name.Contains("ПрименятьСмещениеДоступностиПрослеживаемыхМаркируемыхТоваров")).ValueDouble;

            cmd.Parameters.Add("@P_DaysToShift", SqlDbType.Int);
            cmd.Parameters["@P_DaysToShift"].Value = parameters1C.First(x => x.Name.Contains("КоличествоДнейСмещенияДоступностиПрослеживаемыхМаркируемыхТоваров")).ValueDouble;

            cmd.Parameters.Add("@P_StockPriority", SqlDbType.Int);
            cmd.Parameters["@P_StockPriority"].Value = parameters1C.First(x => x.Name.Contains("ПриоритизироватьСток_64854")).ValueDouble;
            
            cmd.Parameters.Add("@P_YourTimeDelivery", SqlDbType.Int);
            cmd.Parameters["@P_YourTimeDelivery"].Value = deliveryType == Constants.YourTimeDelivery ? 1 : 0;

            cmd.Parameters.Add("@P_IsDelivery", SqlDbType.Int);
            cmd.Parameters["@P_IsDelivery"].Value = deliveryType == Constants.Self ? 0 : 1;

            cmd.Parameters.Add("@P_IsPickup", SqlDbType.Int);
            cmd.Parameters["@P_IsPickup"].Value = deliveryType == Constants.Self ? 1 : 0;

            cmd.CommandTimeout = 5;

            var dateTimeNowOptimizeString = "";
            if (_configuration.GetValue<bool>("optimizeDateTimeNowEveryHour"))
            {
                dateTimeNowOptimizeString = DateMove.ToString("yyyy-MM-ddTHH:00:00");
            }
            else
            {
                dateTimeNowOptimizeString = DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss");
            }

            string useIndexHint = _configuration.GetValue<string>("useIndexHintWarehouseDates");// @", INDEX([_InfoRg23830_Custom2])";
            if (databaseType != "replica_tables")
            {
                useIndexHint = "";
            }

            var pickupPointsList = new List<string>();
            for (int i = 0; i < inputData.PickupPoints.Length; i++)
            {
                var pickupPointString = string.Format("@PickupPoint{0}", i);

                cmd.Parameters.Add(pickupPointString, SqlDbType.NVarChar, 11);
                cmd.Parameters[pickupPointString].Value = inputData.PickupPoints[i];

                pickupPointsList.Add(pickupPointString);
            }

            var pickupPointsString = string.Join(", ", pickupPointsList);

            cmd.CommandText = queryTextBegin + string.Format(query,
                dateTimeNowOptimizeString,
                DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                DateMove.Date.AddDays(parameters1C.First(x => x.Name.Contains("rsp_КоличествоДнейЗаполненияГрафика")).ValueDouble - 1).ToString("yyyy-MM-ddTHH:mm:ss"),
                parameters1C.First(x => x.Name.Contains("КоличествоДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа")).ValueDouble,
                parameters1C.First(x => x.Name.Contains("ПроцентДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа")).ValueDouble,
                useIndexHint,
                pickupPointsString);

            return cmd;
        }

        private static string TextFillGoodsTable(RequestAvailableDeliveryTypes data, SqlCommand cmdGoodsTable)
        {
            var resultString = AvailableDeliveryTypesQueries.GoodsRawCreate;

            var parameters = new List<string>();

            var maxCodes = data.OrderItems.Count;

            for (int i = 0; i < maxCodes; i++)
            {
                RequestAvailableDeliveryTypesItem codesElem;
                codesElem = data.OrderItems[i];

                var article = $"@Article{i}";
                var code = $"@Code{i}";
                var quantity = $"@Quantity{i}";

                var parameterString = $"({article}, {code}, {quantity})";

                cmdGoodsTable.Parameters.Add(article, SqlDbType.NVarChar, 11);
                cmdGoodsTable.Parameters[article].Value = codesElem.Article;

                cmdGoodsTable.Parameters.Add(code, SqlDbType.NVarChar, 11);
                cmdGoodsTable.Parameters[code].Value = String.IsNullOrEmpty(codesElem.Code) ? DBNull.Value : codesElem.Code;

                cmdGoodsTable.Parameters.Add(quantity, SqlDbType.Int, 10);
                cmdGoodsTable.Parameters[quantity].Value = codesElem.Quantity;

                parameters.Add(parameterString);
            }

            if (parameters.Count > 0)
            {
                resultString += string.Format(AvailableDeliveryTypesQueries.GoodsRawInsert, string.Join(", ", parameters));

                parameters.Clear();
            }

            return resultString;
        }

        private List<GlobalParam1C> GetGlobalParameters()
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

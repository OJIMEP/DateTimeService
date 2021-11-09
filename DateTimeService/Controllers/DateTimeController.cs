using AutoMapper;
using DateTimeService.Areas.Identity.Data;
using DateTimeService.Data;
using DateTimeService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DateTimeService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class DateTimeController : ControllerBase
    {
        private readonly ILogger<DateTimeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ILoadBalancing _loadBalacing;
        private readonly IGeoZones _geoZones;
        private readonly IMapper _mapper;

        public DateTimeController(ILogger<DateTimeController> logger, IConfiguration configuration, ILoadBalancing loadBalancing,
                                    IGeoZones geoZones, IMapper mapper)
        {
            _logger = logger;
            _configuration = configuration;
            _loadBalacing = loadBalancing;
            _geoZones = geoZones;
            _mapper = mapper;
        }

        [Authorize(Roles = UserRoles.MaxAvailableCount + "," + UserRoles.Admin)]
        [Route("MaxAvailableCount")]
        [HttpPost]
        public async Task<ObjectResult> MaxAvailableCountAsync(IEnumerable<RequestDataMaxAvailableCount> nomenclatures)
        {


            var dbConnection = await _loadBalacing.GetDatabaseConnectionAsync();
            var conn = dbConnection.Connection;


            var result = new List<ResponseMaxAvailableCount>();
            var logElement = new ElasticLogElement
            {
                Path = HttpContext.Request.Path,
                Host = HttpContext.Request.Host.ToString(),
                RequestContent = JsonSerializer.Serialize(nomenclatures),
                Id = Guid.NewGuid().ToString(),
                DatabaseConnection = LoadBalancing.RemoveCredentialsFromConnectionString(conn.ConnectionString),
                AuthenticatedUser = User.Identity.Name
            };



            long sqlCommandExecutionTime = 0;

            try
            {

                conn.StatisticsEnabled = true;

                string query = @"SELECT
                    T4._Fld3480 AS nomenclature_id,
                    CAST(SUM((T1.Fld21411Balance_ - T1.Fld21412Balance_)) AS NUMERIC(34, 3)) AS max_available_count 
                    FROM (SELECT
                    T2._Fld21408RRef AS Fld21408RRef,
                    CAST(SUM(T2._Fld21412) AS NUMERIC(27, 3)) AS Fld21412Balance_,
                    CAST(SUM(T2._Fld21411) AS NUMERIC(27, 3)) AS Fld21411Balance_
                    FROM dbo._AccumRgT21444 T2 WITH(NOLOCK)
                    LEFT OUTER JOIN dbo._Reference149 T3 WITH(NOLOCK)
                    ON T2._Fld21408RRef = T3._IDRRef
                    WHERE T2._Period = '5999-11-01 00:00:00' AND (((T2._Fld21424 = '2001-01-01 00:00:00') OR (T2._Fld21424 >= @P1)) AND (T3._Fld3480 IN ({0})) AND (T3._Fld3514RRef = 0x84A6131B6DC5555A4627E85757507687)) AND (T2._Fld21412 <> 0 OR T2._Fld21411 <> 0) AND (T2._Fld21412 <> 0 OR T2._Fld21411 <> 0)
                    GROUP BY T2._Fld21408RRef
                    HAVING (CAST(SUM(T2._Fld21412) AS NUMERIC(27, 3))) <> 0.0 OR (CAST(SUM(T2._Fld21411) AS NUMERIC(27, 3))) <> 0.0) T1
                    LEFT OUTER JOIN dbo._Reference149 T4 WITH(NOLOCK)
                    ON T1.Fld21408RRef = T4._IDRRef
                    WHERE ((T1.Fld21411Balance_ - T1.Fld21412Balance_) > 0)
                    GROUP BY T1.Fld21408RRef,
                    T4._Fld3480";


                var DateMove = DateTime.Now.AddMonths(24000);

                //define the SqlCommand object
                SqlCommand cmd = new(query, conn);

                cmd.Parameters.Add("@P1", SqlDbType.DateTime);
                cmd.Parameters["@P1"].Value = DateMove;

                cmd.CommandTimeout = 5;

                var parameters = new string[nomenclatures.Count()];
                for (int i = 0; i < nomenclatures.Count(); i++)
                {
                    parameters[i] = string.Format("@Article{0}", i);
                    cmd.Parameters.AddWithValue(parameters[i], nomenclatures.ToList()[i].Nomenclature_id);
                }

                cmd.CommandText = string.Format(query, string.Join(", ", parameters));



                //execute the SQLCommand
                SqlDataReader dr = cmd.ExecuteReader();

                //check if there are records
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        var resultItem = new ResponseMaxAvailableCount
                        {
                            Nomenclature_id = dr.GetString(0),
                            Max_available_count = dr.GetDecimal(1)
                        };

                        result.Add(resultItem);
                    }
                }

                var stats = conn.RetrieveStatistics();
                sqlCommandExecutionTime = (long)stats["ExecutionTime"];

                //close data reader
                dr.Close();



                logElement.TimeSQLExecution = sqlCommandExecutionTime;
                logElement.ResponseContent = JsonSerializer.Serialize(result);
                logElement.Status = "Ok";
            }
            catch (Exception ex)
            {
                logElement.TimeSQLExecution = sqlCommandExecutionTime;
                logElement.ErrorDescription = ex.Message;
                logElement.Status = "Error";
            }

            conn.Close();

            var logstringElement = JsonSerializer.Serialize(logElement);

            _logger.LogInformation(logstringElement);

            return Ok(result.ToArray());
        }


        [Authorize(Roles = UserRoles.AvailableDate + "," + UserRoles.Admin)]
        [Route("AvailableDate")]
        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> AvailableDateAsync([FromBody] RequestDataAvailableDateByCodeItemsDTO inputDataJson)
        {

            OkObjectResult result;

            var data = _mapper.Map<RequestDataAvailableDate>(inputDataJson);
            string databaseType = "";
            bool customAggs = false;
            Stopwatch stopwatchExecution = new();
            stopwatchExecution.Start();

            var logElementLoadBal = new ElasticLogElement
            {
                Path = HttpContext.Request.Path + (data.CheckQuantity == true ? "_quantity" : ""),
                Host = HttpContext.Request.Host.ToString(),
                RequestContent = JsonSerializer.Serialize(data),
                Id = Guid.NewGuid().ToString(),
                AuthenticatedUser = User.Identity.Name
            };


            //string connString;
            SqlConnection conn;
            Stopwatch watch = new();
            watch.Start();
            try
            {
                //connString = await _loadBalacing.GetDatabaseConnectionAsync();
                var dbConnection = await _loadBalacing.GetDatabaseConnectionAsync();
                conn = dbConnection.Connection;
                databaseType = dbConnection.DatabaseType;
                customAggs = dbConnection.UseAggregations;
            }
            catch (Exception ex)
            {
                //connString = "";
                logElementLoadBal.TimeSQLExecution = 0;
                logElementLoadBal.ErrorDescription = ex.Message;
                logElementLoadBal.Status = "Error";
                var logstringElement1 = JsonSerializer.Serialize(logElementLoadBal);
                _logger.LogInformation(logstringElement1);
                return StatusCode(503);
            }
            watch.Stop();


            if (conn == null)
            {
                logElementLoadBal.TimeSQLExecution = 0;
                logElementLoadBal.ErrorDescription = "Не найдено доступное соединение к БД";
                logElementLoadBal.Status = "Error";
                logElementLoadBal.LoadBalancingExecution = watch.ElapsedMilliseconds;
                var logstringElement1 = JsonSerializer.Serialize(logElementLoadBal);
                _logger.LogInformation(logstringElement1);

                Dictionary<string, string> errorDesc = new();
                errorDesc.Add("ErrorDescription", "Не найдено доступное соединение к БД");

                return StatusCode(500, errorDesc);
            }

            ResponseAvailableDate dbResult = new();

            var logElement = new ElasticLogElement
            {
                Path = HttpContext.Request.Path + (data.CheckQuantity == true ? "_quantity" : ""),
                Host = HttpContext.Request.Host.ToString(),
                RequestContent = JsonSerializer.Serialize(data),
                Id = Guid.NewGuid().ToString(),
                DatabaseConnection = LoadBalancing.RemoveCredentialsFromConnectionString(conn.ConnectionString),
                AuthenticatedUser = User.Identity.Name,
                LoadBalancingExecution = watch.ElapsedMilliseconds
            };

            logElement.AdditionalData.Add("Referer", Request.Headers["Referer"].ToString());
            logElement.AdditionalData.Add("User-Agent", Request.Headers["User-Agent"].ToString());
            logElement.AdditionalData.Add("RemoteIpAddress", Request.HttpContext.Connection.RemoteIpAddress.ToString());


            var dataErrors = data.LogicalCheckInputData();
            if (dataErrors.Count > 0)
            {
                logElement.TimeSQLExecution = 0;
                logElement.ErrorDescription = "Некорректные входные данные";
                logElement.Status = "Error";
                logElement.AdditionalData.Add("InputErrors", JsonSerializer.Serialize(dataErrors));
                var logstringElement1 = JsonSerializer.Serialize(logElement);

                _logger.LogInformation(logstringElement1);

                return StatusCode(400, dataErrors);
            }

            watch.Reset();

            var Parameters1C = new List<GlobalParam1C>
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
                    Name = "ПрименятьСмещениеДоступностиПрослеживаемыхМаркируемыхТоваров",
                    DefaultDouble = 0
                },
                new GlobalParam1C
                {
                    Name = "КоличествоДнейСмещенияДоступностиПрослеживаемыхМаркируемыхТоваров",
                    DefaultDouble = 0
                }
            };
            watch.Start();
            GlobalParam1C.FillValues(conn, Parameters1C, _logger);
            watch.Stop();
            logElement.GlobalParametersExecution = watch.ElapsedMilliseconds;
            watch.Reset();

            long sqlCommandExecutionTime = 0;
            watch.Start();
            try
            {
                if (data.Codes.Length == 0 || (data.Codes.Length == 1 && data.Codes[0] == null))
                {
                    throw new Exception("Пустой массив кодов");
                }

                //sql connection object
                //using SqlConnection conn = new(connString);

                conn.StatisticsEnabled = true;

                //open connection
                //conn.Open();
                string query = "";

                List<string> queryParts=new();

                queryParts.Add(data.CheckQuantity == true ? Queries.AvailableDateWithCount1 : Queries.AvailableDate1);
                queryParts.Add(customAggs == true ? Queries.AvailableDate2MinimumWarehousesCustom : Queries.AvailableDate2MinimumWarehousesBasic);
                queryParts.Add(data.CheckQuantity == true ? Queries.AvailableDateWithCount3 : Queries.AvailableDate3);
                queryParts.Add(customAggs == true ? Queries.AvailableDate4IntervalsCustom : Queries.AvailableDate4IntervalsBasic);
                queryParts.Add(Queries.AvailableDate5);
                queryParts.Add(customAggs == true ? Queries.AvailableDate6DeliveryPowerCustom : Queries.AvailableDate6DeliveryPowerBasic);
                queryParts.Add(Queries.AvailableDate7);

                query = String.Join("", queryParts);

                var DateMove = DateTime.Now.AddMonths(24000);
                var TimeNow = new DateTime(2001, 1, 1, DateMove.Hour, DateMove.Minute, DateMove.Second);
                var EmptyDate = new DateTime(2001, 1, 1, 0, 0, 0);
                var MaxDate = new DateTime(5999, 11, 11, 0, 0, 0);

                SqlCommand cmd = new(query, conn);


                List<string> pickups = new();

                var queryTextBegin = TextFillGoodsTable(data, cmd, true, pickups);

                if (_configuration.GetValue<bool>("disableKeepFixedPlan"))
                {
                    query = query.Replace(", KEEPFIXED PLAN", "");
                    queryTextBegin = queryTextBegin.Replace(", KEEPFIXED PLAN", "");
                }

                //define the SqlCommand object
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


                cmd.Parameters.Add("@P_CityCode", SqlDbType.NVarChar, 10);
                cmd.Parameters["@P_CityCode"].Value = data.CityId;

                cmd.Parameters.Add("@P_DateTimeNow", SqlDbType.DateTime);
                cmd.Parameters["@P_DateTimeNow"].Value = DateMove;

                cmd.Parameters.Add("@P_DateTimePeriodBegin", SqlDbType.DateTime);
                cmd.Parameters["@P_DateTimePeriodBegin"].Value = DateMove.Date;

                cmd.Parameters.Add("@P_DateTimePeriodEnd", SqlDbType.DateTime);
                cmd.Parameters["@P_DateTimePeriodEnd"].Value = DateMove.Date.AddDays(Parameters1C.First(x => x.Name.Contains("rsp_КоличествоДнейЗаполненияГрафика")).ValueDouble - 1);

                cmd.Parameters.Add("@P_TimeNow", SqlDbType.DateTime);
                cmd.Parameters["@P_TimeNow"].Value = TimeNow;

                cmd.Parameters.Add("@P_EmptyDate", SqlDbType.DateTime);
                cmd.Parameters["@P_EmptyDate"].Value = EmptyDate;

                cmd.Parameters.Add("@P_MaxDate", SqlDbType.DateTime);
                cmd.Parameters["@P_MaxDate"].Value = MaxDate;

                cmd.Parameters.Add("@P_DaysToShow", SqlDbType.Int);
                cmd.Parameters["@P_DaysToShow"].Value = 7;

                cmd.CommandTimeout = 5;


                var dateTimeNowOptimizeString = "";
                if (_configuration.GetValue<bool>("optimizeDateTimeNowEveryHour"))
                {
                    dateTimeNowOptimizeString = DateMove.ToString("yyyy-MM-ddTHH:00:00");
                }
                else
                {
                    dateTimeNowOptimizeString = dateTimeNowOptimizeString = DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss");
                }

                var pickupWorkingHoursJoinType = _configuration.GetValue<string>("pickupWorkingHoursJoinType");

                string useIndexHint = _configuration.GetValue<string>("useIndexHintWarehouseDates");// @", INDEX([_InfoRg23830_Custom2])";
                if (databaseType != "replica_tables" || customAggs)
                {
                    useIndexHint = "";
                }

                cmd.CommandText = queryTextBegin + string.Format(query, string.Join(",", pickupParameters),
                    dateTimeNowOptimizeString,
                    DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                    DateMove.Date.AddDays(Parameters1C.First(x => x.Name.Contains("rsp_КоличествоДнейЗаполненияГрафика")).ValueDouble - 1).ToString("yyyy-MM-ddTHH:mm:ss"),
                    Parameters1C.First(x => x.Name.Contains("КоличествоДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа")).ValueDouble,
                    Parameters1C.First(x => x.Name.Contains("ПроцентДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа")).ValueDouble,
                    pickupWorkingHoursJoinType,
                    useIndexHint,
                    Parameters1C.First(x => x.Name.Contains("ПрименятьСмещениеДоступностиПрослеживаемыхМаркируемыхТоваров")).ValueDouble,
                    Parameters1C.First(x => x.Name.Contains("КоличествоДнейСмещенияДоступностиПрослеживаемыхМаркируемыхТоваров")).ValueDouble);


                //execute the SQLCommand
                SqlDataReader dr = cmd.ExecuteReader();

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

                var stats = conn.RetrieveStatistics();
                sqlCommandExecutionTime = (long)stats["ExecutionTime"];

                //close data reader
                dr.Close();

                //close connection
                //conn.Close();

                logElement.TimeSQLExecution = sqlCommandExecutionTime;
                logElement.ResponseContent = JsonSerializer.Serialize(dbResult);
                logElement.Status = "Ok";
                logElement.AdditionalData.Add("stats", JsonSerializer.Serialize(stats));
            }
            catch (Exception ex)
            {
                logElement.TimeSQLExecution = sqlCommandExecutionTime;
                logElement.ErrorDescription = ex.Message;
                logElement.Status = "Error";
            }

            conn.Close();

            var resultDict = new ResponseAvailableDateDict();

            try
            {
                foreach (var codeItem in data.Codes)
                {

                    if (data.Codes.Length == 0 || (data.Codes.Length == 1 && data.Codes[0] == null))
                    {
                        break;
                    }

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

                    resultElement.Courier = data.DeliveryTypes.Contains("courier") && dbResult.Courier[dbResultIndex].Year != 3999
                        ? dbResult.Courier[dbResultIndex].Date.ToString("yyyy-MM-ddTHH:mm:ss")
                        : null;
                    resultElement.Self = data.DeliveryTypes.Contains("self") && dbResult.Self[dbResultIndex].Year != 3999
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
                logElement.TimeSQLExecution = sqlCommandExecutionTime;
                logElement.ErrorDescription = "Duplicated keys in dictionary";
                logElement.Status = "Error";
                logElement.AdditionalData.Add("Key", ex.Message);
            }
            catch (Exception ex)
            {
                logElement.TimeSQLExecution = sqlCommandExecutionTime;
                logElement.ErrorDescription = ex.Message;
                logElement.Status = "Error";
            }



            if (data.DeliveryTypes.Contains("self") && data.DeliveryTypes.Contains("courier"))
            {
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

                result = Ok(resultBoth);
            }
            else
            {
                result = Ok(resultDict);
            }

            watch.Stop();
            logElement.TimeSQLExecutionFact = watch.ElapsedMilliseconds;

            stopwatchExecution.Stop();
            logElement.TimeFullExecution = stopwatchExecution.ElapsedMilliseconds;

            var logstringElement = JsonSerializer.Serialize(logElement);

            _logger.LogInformation(logstringElement);

            return result;
        }



        [Authorize(Roles = UserRoles.IntervalList + "," + UserRoles.Admin)]
        [Route("IntervalList")]
        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> IntervalListAsync(RequestIntervalListDTO inputData)
        {
            var data = _mapper.Map<RequestIntervalList>(inputData);
            string databaseType = "";
            bool customAggs = false;
            Stopwatch stopwatchExecution = new();
            stopwatchExecution.Start();

            

            

            var logElementLoadBal = new ElasticLogElement
            {
                Path = HttpContext.Request.Path,
                Host = HttpContext.Request.Host.ToString(),
                RequestContent = JsonSerializer.Serialize(data),
                Id = Guid.NewGuid().ToString(),
                AuthenticatedUser = User.Identity.Name
            };

            //string connString;
            SqlConnection conn;
            Stopwatch watch = new();
            watch.Start();
            try
            {
                //connString = await _loadBalacing.GetDatabaseConnectionAsync();
                //conn = await _loadBalacing.GetDatabaseConnectionAsync("");
                var dbConnection = await _loadBalacing.GetDatabaseConnectionAsync();
                conn = dbConnection.Connection;
                databaseType = dbConnection.DatabaseType;
                customAggs = dbConnection.UseAggregations;
            }
            catch (Exception ex)
            {
                //connString = "";
                logElementLoadBal.TimeSQLExecution = 0;
                logElementLoadBal.ErrorDescription = ex.Message;
                logElementLoadBal.Status = "Error";
                var logstringElement1 = JsonSerializer.Serialize(logElementLoadBal);
                _logger.LogInformation(logstringElement1);
                return StatusCode(500);
            }
            watch.Stop();


            if (conn == null)
            {
                logElementLoadBal.TimeSQLExecution = 0;
                logElementLoadBal.ErrorDescription = "Не найдено доступное соединение к БД";
                logElementLoadBal.Status = "Error";
                logElementLoadBal.LoadBalancingExecution = watch.ElapsedMilliseconds;
                var logstringElement1 = JsonSerializer.Serialize(logElementLoadBal);
                _logger.LogInformation(logstringElement1);

                Dictionary<string, string> errorDesc = new();
                errorDesc.Add("ErrorDescription", "Не найдено доступное соединение к БД");

                return StatusCode(500, errorDesc);
            }

            ResponseIntervalList result = new();


            var logElement = new ElasticLogElement
            {
                Path = HttpContext.Request.Path,
                Host = HttpContext.Request.Host.ToString(),
                RequestContent = JsonSerializer.Serialize(inputData),
                Id = Guid.NewGuid().ToString(),
                DatabaseConnection = LoadBalancing.RemoveCredentialsFromConnectionString(conn.ConnectionString),
                AuthenticatedUser = User.Identity.Name,
                LoadBalancingExecution = watch.ElapsedMilliseconds
            };

            logElement.AdditionalData.Add("Referer", Request.Headers["Referer"].ToString());
            logElement.AdditionalData.Add("User-Agent", Request.Headers["Referer"].ToString());
            logElement.AdditionalData.Add("RemoteIpAddress", Request.HttpContext.Connection.RemoteIpAddress.ToString());

            var dataErrors = data.LogicalCheckInputData();
            if (dataErrors.Count > 0)
            {
                logElement.TimeSQLExecution = 0;
                logElement.ErrorDescription = "Некорректные входные данные";
                logElement.Status = "Error";
                logElement.AdditionalData.Add("InputErrors", JsonSerializer.Serialize(dataErrors));
                var logstringElement1 = JsonSerializer.Serialize(logElement);

                _logger.LogInformation(logstringElement1);

                return StatusCode(400, dataErrors);
            }

            watch.Reset();

            var Parameters1C = new List<GlobalParam1C>
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
                    DefaultDouble = 4
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
                }
            };

            watch.Start();
            GlobalParam1C.FillValues(conn, Parameters1C, _logger);
            watch.Stop();
            logElement.GlobalParametersExecution = watch.ElapsedMilliseconds;
            watch.Reset();

            long sqlCommandExecutionTime = 0;


            string zoneId = "";

            bool checkByOrder = !String.IsNullOrEmpty(data.OrderNumber) && data.OrderDate != default;

            bool alwaysCheckGeozone = false;

            bool adressExists = false;

            if (data.DeliveryType == "self" || checkByOrder)
            {
                adressExists = true;
                alwaysCheckGeozone = false;
            }
            else
            {
                alwaysCheckGeozone = _configuration.GetValue<bool>("alwaysCheckGeozone");
                if (!alwaysCheckGeozone)
                {
                    adressExists = _geoZones.AdressExists(conn, data.AddressId);
                }
            }

            if (!adressExists || alwaysCheckGeozone)
            {
                //TODO: добавить обращение к сервисам для получения геозоны
                Stopwatch stopwatch = new();
                stopwatch.Start();
                var coords = await _geoZones.GetAddressCoordinates(data.AddressId);
                stopwatch.Stop();
                logElement.TimeLocationExecution = stopwatch.ElapsedMilliseconds;
                stopwatch.Reset();
                stopwatch.Start();
                if (coords.AvailableToUse)
                {
                    zoneId = await _geoZones.GetGeoZoneID(coords);
                }
                stopwatch.Stop();
                logElement.TimeBtsExecution = stopwatch.ElapsedMilliseconds;
            }

            watch.Start();

            if (!adressExists && zoneId == "")
            {
                logElement.TimeSQLExecution = 0;
                logElement.ErrorDescription = "Адрес и геозона не найдены!";
                logElement.Status = "Error";
            }
            else
                try
                {
                    //sql connection object
                    //using SqlConnection conn = new(connString);

                    conn.StatisticsEnabled = true;

                    //open connection
                    //conn.Open();

                    string query = Queries.IntervalList;

                    SqlCommand cmd = new(query, conn);

                    var queryTextBegin = TextFillGoodsTable(data, cmd, false);

                    if (_configuration.GetValue<bool>("disableKeepFixedPlan"))
                    {
                        query = query.Replace(", KEEPFIXED PLAN", "");
                        queryTextBegin = queryTextBegin.Replace(", KEEPFIXED PLAN", "");
                    }

                    var DateMove = DateTime.Now.AddMonths(24000);
                    var TimeNow = new DateTime(2001, 1, 1, DateMove.Hour, DateMove.Minute, DateMove.Second);
                    var EmptyDate = new DateTime(2001, 1, 1, 0, 0, 0);
                    var MaxDate = new DateTime(5999, 11, 11, 0, 0, 0);

                    cmd.Parameters.Add("@P_AdressCode", SqlDbType.NVarChar, 20);
                    cmd.Parameters["@P_AdressCode"].Value = data.AddressId != null ? data.AddressId : DBNull.Value;

                    cmd.Parameters.Add("@PickupPoint1", SqlDbType.NVarChar, 5);
                    cmd.Parameters["@PickupPoint1"].Value = data.PickupPoint != null ? data.PickupPoint : DBNull.Value;

                    cmd.Parameters.Add("@P_Credit", SqlDbType.Int);
                    cmd.Parameters["@P_Credit"].Value = data.Payment == "partly_pay" ? 1 : 0;

                    cmd.Parameters.Add("@P_Floor", SqlDbType.Float);
                    cmd.Parameters["@P_Floor"].Value = (double)(data.Floor != null ? data.Floor : Parameters1C.First(x => x.Name.Contains("Логистика_ЭтажПоУмолчанию")).ValueDouble);

                    cmd.Parameters.Add("@P_DaysToShow", SqlDbType.Int);
                    cmd.Parameters["@P_DaysToShow"].Value = 7;

                    cmd.Parameters.Add("@P_DateTimeNow", SqlDbType.DateTime);
                    cmd.Parameters["@P_DateTimeNow"].Value = DateMove;

                    cmd.Parameters.Add("@P_DateTimePeriodBegin", SqlDbType.DateTime);
                    cmd.Parameters["@P_DateTimePeriodBegin"].Value = DateMove.Date;

                    cmd.Parameters.Add("@P_DateTimePeriodEnd", SqlDbType.DateTime);
                    cmd.Parameters["@P_DateTimePeriodEnd"].Value = DateMove.Date.AddDays(Parameters1C.First(x => x.Name.Contains("rsp_КоличествоДнейЗаполненияГрафика")).ValueDouble - 1);

                    cmd.Parameters.Add("@P_TimeNow", SqlDbType.DateTime);
                    cmd.Parameters["@P_TimeNow"].Value = TimeNow;

                    cmd.Parameters.Add("@P_EmptyDate", SqlDbType.DateTime);
                    cmd.Parameters["@P_EmptyDate"].Value = EmptyDate;

                    cmd.Parameters.Add("@P_MaxDate", SqlDbType.DateTime);
                    cmd.Parameters["@P_MaxDate"].Value = MaxDate;

                    cmd.Parameters.Add("@P_GeoCode", SqlDbType.NVarChar);
                    cmd.Parameters["@P_GeoCode"].Value = zoneId;

                    cmd.Parameters.Add("@P_OrderDate", SqlDbType.DateTime);
                    cmd.Parameters["@P_OrderDate"].Value = data.OrderDate.AddMonths(24000);

                    cmd.Parameters.Add("@P_OrderNumber", SqlDbType.NVarChar, 11);
                    cmd.Parameters["@P_OrderNumber"].Value = data.OrderNumber != null ? data.OrderNumber : DBNull.Value;

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

                    cmd.CommandText = queryTextBegin + string.Format(query, "",
                        dateTimeNowOptimizeString,
                        DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                        DateMove.Date.AddDays(Parameters1C.First(x => x.Name.Contains("rsp_КоличествоДнейЗаполненияГрафика")).ValueDouble - 1).ToString("yyyy-MM-ddTHH:mm:ss"),
                        Parameters1C.First(x => x.Name.Contains("КоличествоДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа")).ValueDouble,
                        Parameters1C.First(x => x.Name.Contains("ПроцентДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа")).ValueDouble,
                        useIndexHint,
                        Parameters1C.First(x => x.Name.Contains("ПрименятьСмещениеДоступностиПрослеживаемыхМаркируемыхТоваров")).ValueDouble,
                        Parameters1C.First(x => x.Name.Contains("КоличествоДнейСмещенияДоступностиПрослеживаемыхМаркируемыхТоваров")).ValueDouble);



                    //execute the SQLCommand
                    SqlDataReader dr = cmd.ExecuteReader();

                    var resultLog = new ResponseIntervalListWithOffSet();

                    //check if there are records
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

                            resultLog.Data.Add(new ResponseIntervalListElementWithOffSet
                            {
                                Begin = new(begin),
                                End = new(end),
                                Bonus = bonus
                            });
                        }
                    }

                    var stats = conn.RetrieveStatistics();
                    sqlCommandExecutionTime = (long)stats["ExecutionTime"];

                    //close data reader
                    dr.Close();

                    //close connection
                    //conn.Close();

                    logElement.TimeSQLExecution = sqlCommandExecutionTime;
                    logElement.ResponseContent = JsonSerializer.Serialize(resultLog);
                    logElement.Status = "Ok";
                }
                catch (Exception ex)
                {
                    logElement.TimeSQLExecution = sqlCommandExecutionTime;
                    logElement.ErrorDescription = ex.Message;
                    logElement.Status = "Error";
                }

            conn.Close();
            watch.Stop();
            logElement.TimeSQLExecutionFact = watch.ElapsedMilliseconds;

            stopwatchExecution.Stop();
            logElement.TimeFullExecution = stopwatchExecution.ElapsedMilliseconds;

            var logstringElement = JsonSerializer.Serialize(logElement);

            _logger.LogInformation(logstringElement);

            return Ok(result);
        }


        private static string TextFillGoodsTable(RequestIntervalList data, SqlCommand cmdGoodsTable, bool optimizeRowsCount)
        {
            RequestDataAvailableDate convertedData = new()
            {
                Codes = data.OrderItems.ToArray()
            };

            return TextFillGoodsTable(convertedData, cmdGoodsTable, optimizeRowsCount, new());
        }

        public static string TextFillGoodsTable(RequestDataAvailableDate data, SqlCommand cmdGoodsTable, bool optimizeRowsCount, List<string> PickupsList)
        {

            var resultString = Queries.CreateTableGoodsRawCreate;

            var insertRowsLimit = 900;

            var parameters = new List<string>();

            var maxCodes = data.Codes.Length;

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

            if (data.Codes.Length > 2) maxCodes = 10;
            if (data.Codes.Length > 10) maxCodes = 30;
            if (data.Codes.Length > 30) maxCodes = 60;
            if (data.Codes.Length > 60) maxCodes = 100;
            if (data.Codes.Length > maxCodes || !optimizeRowsCount) maxCodes = data.Codes.Length;


            for (int codesCounter = 0; codesCounter < maxCodes; codesCounter++)
            {

                RequestDataCodeItem codesElem;
                if (codesCounter < data.Codes.Length)
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

    }
}

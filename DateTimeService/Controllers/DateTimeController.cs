using DateTimeService.Areas.Identity.Data;
using DateTimeService.Data;
using DateTimeService.Logging;
using DateTimeService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;

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

            //string connString = _configuration.GetConnectionString("1CDataSqlConnection");

            string connString = await _loadBalacing.GetDatabaseConnectionAsync();

            //string connString = @"Server=localhost;Database=DevBase_cut_v3;Uid=sa;Pwd=; Trusted_Connection = False;";

            var result = new List<ResponseMaxAvailableCount>();
            var logElement = new ElasticLogElement
            {
                Path = HttpContext.Request.Path,
                Host = HttpContext.Request.Host.ToString(),
                RequestContent = JsonSerializer.Serialize(nomenclatures),
                Id = Guid.NewGuid().ToString(),
                DatabaseConnection = LoadBalancing.RemoveCredentialsFromConnectionString(connString),
                AuthenticatedUser = User.Identity.Name
            };



            long sqlCommandExecutionTime = 0;

            try
            {
                //sql connection object
                using SqlConnection conn = new(connString);

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

                //open connection
                conn.Open();

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

                //close connection
                conn.Close();

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

            var logstringElement = JsonSerializer.Serialize(logElement);

            _logger.LogInformation(logstringElement);

            return Ok(result.ToArray());
        }


        [Authorize(Roles = UserRoles.AvailableDate + "," + UserRoles.Admin)]
        [Route("AvailableDate")]
        [HttpPost]
        public async Task<IActionResult> AvailableDateAsync([FromBody] JsonElement inputDataJson)
        {
            RequestDataAvailableDate data = new();
            try //здесь по хорошему нужно проверить на схему, но т.к. решение временное применим просто трай кэтч
            {
                var inputDataByCodeItems = JsonSerializer.Deserialize<RequestDataAvailableDateByCodeItemsDTO>(inputDataJson.ToString());
                data = _mapper.Map<RequestDataAvailableDate>(inputDataByCodeItems);
            }
            catch
            {
                var inputDataByCodes = JsonSerializer.Deserialize<RequestDataAvailableDateByCodesDTO>(inputDataJson.ToString());
                data = _mapper.Map<RequestDataAvailableDate>(inputDataByCodes);
            }

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


            string connString;
            Stopwatch watch = new();
            watch.Start();
            try
            {
                connString = await _loadBalacing.GetDatabaseConnectionAsync();
            }
            catch (Exception ex)
            {
                connString = "";
                logElementLoadBal.TimeSQLExecution = 0;
                logElementLoadBal.ErrorDescription = ex.Message;
                logElementLoadBal.Status = "Error";
                var logstringElement1 = JsonSerializer.Serialize(logElementLoadBal);
                _logger.LogInformation(logstringElement1);
            }
            watch.Stop();
            //string connString = @"Server=localhost;Database=DevBase_cut_v3;Uid=sa;Pwd=; Trusted_Connection = False;";

            ResponseAvailableDate dbResult = new();

            var logElement = new ElasticLogElement
            {
                Path = HttpContext.Request.Path,
                Host = HttpContext.Request.Host.ToString(),
                RequestContent = JsonSerializer.Serialize(data),
                Id = Guid.NewGuid().ToString(),
                DatabaseConnection = LoadBalancing.RemoveCredentialsFromConnectionString(connString),
                AuthenticatedUser = User.Identity.Name,
                LoadBalancingExecution = watch.ElapsedMilliseconds
            };

            logElement.AdditionalData.Add("Referer",Request.Headers["Referer"].ToString());
            logElement.AdditionalData.Add("User-Agent", Request.Headers["User-Agent"].ToString());
            logElement.AdditionalData.Add("RemoteIpAddress", Request.HttpContext.Connection.RemoteIpAddress.ToString()); 


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
                }
            };
            watch.Start();
            GlobalParam1C.FillValues(connString, Parameters1C, _logger);
            watch.Stop();
            logElement.GlobalParametersExecution = watch.ElapsedMilliseconds;
            watch.Reset();

            long sqlCommandExecutionTime = 0;
            watch.Start();
            try
            {
                if (data.codes.Length == 0 || (data.codes.Length == 1 && data.codes[0] == null))
                {
                    throw new Exception("Пустой массив кодов");
                }

                //sql connection object
                using SqlConnection conn = new(connString);

                conn.StatisticsEnabled = true;

                //open connection
                conn.Open();

                string query = Queries.AvailableDate;

                var DateMove = DateTime.Now.AddMonths(24000);
                var TimeNow = new DateTime(2001, 1, 1, DateMove.Hour, DateMove.Minute, DateMove.Second);
                var EmptyDate = new DateTime(2001, 1, 1, 0, 0, 0);
                var MaxDate = new DateTime(5999, 11, 11, 0, 0, 0);

                SqlCommand cmd = new(query, conn);

                //FillGoodsTable(data, conn);

                var queryTextBegin = TextFillGoodsTable(data, cmd, true);

                if (_configuration.GetValue<bool>("disableKeepFixedPlan"))
                {
                    query = query.Replace(", KEEPFIXED PLAN", "");
                    queryTextBegin = queryTextBegin.Replace(", KEEPFIXED PLAN", "");
                }

                //define the SqlCommand object
                

                cmd.Parameters.Add("@P_CityCode", SqlDbType.NVarChar, 10);
                cmd.Parameters["@P_CityCode"].Value = data.city_id;

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



                //var count = data.codes.Length > 60 ? data.codes.Length : 60;
                //var articleParameters = new string[count];
                //var codeParameters = new string[count];

                //for (int i = 0; i < count; i++)
                //{
                //    articleParameters[i] = string.Format("@Article{0}", i);
                //    codeParameters[i]    = string.Format("@Code{0}", i);
                //    cmd.Parameters.Add(articleParameters[i], SqlDbType.NVarChar,10);
                //    cmd.Parameters.Add(codeParameters[i], SqlDbType.NVarChar, 11);

                //    if (i >= data.codes.Length)
                //    {
                //        cmd.Parameters[articleParameters[i]].Value = DBNull.Value;
                //        cmd.Parameters[codeParameters[i]].Value = DBNull.Value;
                //    }
                //    else
                //    {
                //        if (String.IsNullOrEmpty(data.codes[i].article))
                //            cmd.Parameters[articleParameters[i]].Value = DBNull.Value;
                //        else
                //            cmd.Parameters[articleParameters[i]].Value = data.codes[i].article;
                //        if (String.IsNullOrEmpty(data.codes[i].code))
                //            cmd.Parameters[codeParameters[i]].Value = DBNull.Value;
                //        else
                //            cmd.Parameters[codeParameters[i]].Value = data.codes[i].code;
                //    }
                //}

                var dateTimeNowOptimizeString = "";
                if (_configuration.GetValue<bool>("optimizeDateTimeNowEveryHour"))
                {
                    dateTimeNowOptimizeString = DateMove.ToString("yyyy-MM-ddTHH:00:00");
                }
                else
                {
                    dateTimeNowOptimizeString = dateTimeNowOptimizeString = DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss"); 
                }

                cmd.CommandText = queryTextBegin +  string.Format(query, "",
                    dateTimeNowOptimizeString,
                    DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                    DateMove.Date.AddDays(Parameters1C.First(x => x.Name.Contains("rsp_КоличествоДнейЗаполненияГрафика")).ValueDouble - 1).ToString("yyyy-MM-ddTHH:mm:ss"),
                    Parameters1C.First(x => x.Name.Contains("КоличествоДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа")).ValueDouble,
                    Parameters1C.First(x => x.Name.Contains("ПроцентДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа")).ValueDouble);//,
                                                                                                                           //string.Join(", ", codeParameters));


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

                        dbResult.article.Add(article);
                        dbResult.code.Add(code);
                        dbResult.courier.Add(new(availableDateCourier));
                        dbResult.self.Add(new(availableDateSelf));
                    }
                }

                var stats = conn.RetrieveStatistics();
                sqlCommandExecutionTime = (long)stats["ExecutionTime"];

                //close data reader
                dr.Close();

                //close connection
                conn.Close();

                logElement.TimeSQLExecution = sqlCommandExecutionTime;
                logElement.ResponseContent = JsonSerializer.Serialize(dbResult);
                logElement.Status = "Ok";
            }
            catch (Exception ex)
            {
                logElement.TimeSQLExecution = sqlCommandExecutionTime;
                logElement.ErrorDescription = ex.Message;
                logElement.Status = "Error";
            }

            var resultDict = new ResponseAvailableDateDict();

            try
            {
                foreach (var codeItem in data.codes)
                {

                    if (data.codes.Length == 0 || (data.codes.Length == 1 && data.codes[0] == null))
                    {
                        break;
                    }

                    var resultElement = new ResponseAvailableDateDictElement
                    {
                        code = codeItem.article,
                        sales_code = codeItem.sales_code,
                        courier = null,
                        self = null
                    };

                    int dbResultIndex = -1;
                    if (String.IsNullOrEmpty(codeItem.code))
                    {
                        dbResultIndex = dbResult.article.FindIndex(s => s == codeItem.article);
                    }
                    else
                    {
                        dbResultIndex = dbResult.code.FindIndex(s => s == codeItem.code);
                    }

                    if (dbResultIndex == -1)
                        continue;

                    resultElement.courier = data.delivery_types.Contains("courier")
                        ? dbResult.courier[dbResultIndex].Date.ToString("yyyy-MM-ddTHH:mm:ss")
                        : null;
                    resultElement.self = data.delivery_types.Contains("self")
                        ? dbResult.self[dbResultIndex].Date.ToString("yyyy-MM-ddTHH:mm:ss")
                        : null;

                    if (String.IsNullOrEmpty(codeItem.code))
                    {
                        resultDict.data.Add(codeItem.article, resultElement);
                    }
                    else
                    {
                        resultDict.data.Add(String.Concat(codeItem.article, "_", codeItem.sales_code), resultElement);
                    }
                }
            }
            catch(Exception ex)
            {
                logElement.TimeSQLExecution = sqlCommandExecutionTime;
                logElement.ErrorDescription = ex.Message;
                logElement.Status = "Error";
            }
            
            
            watch.Stop();
            logElement.TimeSQLExecutionFact = watch.ElapsedMilliseconds;

            stopwatchExecution.Stop();
            logElement.TimeFullExecution = stopwatchExecution.ElapsedMilliseconds;

            var logstringElement = JsonSerializer.Serialize(logElement);

            _logger.LogInformation(logstringElement);

            return Ok(resultDict);
        }



        [Authorize(Roles = UserRoles.IntervalList + "," + UserRoles.Admin)]
        [Route("IntervalList")]
        [HttpPost]
        public async Task<IActionResult> IntervalListAsync(RequestIntervalListDTO inputData)
        {
            var data = _mapper.Map<RequestIntervalList>(inputData);

            Stopwatch stopwatchExecution = new();
            stopwatchExecution.Start();

            Stopwatch watch = new();
            watch.Start();
            string connString = await _loadBalacing.GetDatabaseConnectionAsync();
            watch.Stop();

            //string connString = _configuration.GetConnectionString("1CDataSqlConnection");

            //string connString = @"Server=localhost;Database=DevBase_cut_v3;Uid=sa;Pwd=; Trusted_Connection = False;";

            ResponseIntervalList result = new();


            var logElement = new ElasticLogElement
            {
                Path = HttpContext.Request.Path,
                Host = HttpContext.Request.Host.ToString(),
                RequestContent = JsonSerializer.Serialize(inputData),
                Id = Guid.NewGuid().ToString(),
                DatabaseConnection = LoadBalancing.RemoveCredentialsFromConnectionString(connString),
                AuthenticatedUser = User.Identity.Name,
                LoadBalancingExecution = watch.ElapsedMilliseconds
            };

            logElement.AdditionalData.Add("Referer", Request.Headers["Referer"].ToString());
            logElement.AdditionalData.Add("User-Agent", Request.Headers["Referer"].ToString());
            logElement.AdditionalData.Add("RemoteIpAddress", Request.HttpContext.Connection.RemoteIpAddress.ToString());

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
                }
            };

            watch.Start();
            GlobalParam1C.FillValues(connString, Parameters1C, _logger);
            watch.Stop();
            logElement.GlobalParametersExecution = watch.ElapsedMilliseconds;
            watch.Reset();

            long sqlCommandExecutionTime = 0;


            string zoneId = "";


            bool alwaysCheckGeozone = false;

            bool adressExists = false;

            if (data.delivery_type == "self")
            {
                adressExists = true;
                alwaysCheckGeozone = false;
            }
            else
            {
                alwaysCheckGeozone = _configuration.GetValue<bool>("alwaysCheckGeozone");

                adressExists = _geoZones.AdressExists(connString, data.address_id);
            }

            if (!adressExists || alwaysCheckGeozone)
            {
                //TODO: добавить обращение к сервисам для получения геозоны
                Stopwatch stopwatch = new();
                stopwatch.Start();
                var coords = await _geoZones.GetAddressCoordinates(data.address_id);
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
                    using SqlConnection conn = new(connString);

                    conn.StatisticsEnabled = true;

                    //open connection
                    conn.Open();

                    string query = Queries.IntervalList;

                    SqlCommand cmd = new(query, conn);

                    //FillGoodsTable(data, conn);

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

                    //define the SqlCommand object
                    //SqlCommand cmd = new(query, conn);

                    cmd.Parameters.Add("@P_AdressCode", SqlDbType.NVarChar, 20);
                    cmd.Parameters["@P_AdressCode"].Value = data.address_id != null ? data.address_id : DBNull.Value;

                    cmd.Parameters.Add("@PickupPoint1", SqlDbType.NVarChar,5);
                    cmd.Parameters["@PickupPoint1"].Value = data.pickup_point != null ? data.pickup_point : DBNull.Value;

                    cmd.Parameters.Add("@P_Credit", SqlDbType.Int);
                    cmd.Parameters["@P_Credit"].Value = data.payment == "partly_pay" ? 1 : 0;

                    cmd.Parameters.Add("@P_Floor", SqlDbType.Float);
                    cmd.Parameters["@P_Floor"].Value = (double)(data.floor != null ? data.floor : Parameters1C.First(x => x.Name.Contains("Логистика_ЭтажПоУмолчанию")).ValueDouble);

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

                    cmd.CommandTimeout = 5;

                    //var parameters = new string[data.orderItems.Count];
                    //for (int i = 0; i < data.orderItems.Count; i++)
                    //{
                    //    parameters[i] = string.Format("(@Article{0}, @Code{0}, @Quantity{0})", i);

                    //    cmd.Parameters.AddWithValue(string.Format("@Article{0}", i), data.orderItems[i].article);
                    //    if (String.IsNullOrEmpty(data.orderItems[i].code))
                    //        cmd.Parameters.AddWithValue(string.Format("@Code{0}", i), DBNull.Value);
                    //    else
                    //        cmd.Parameters.AddWithValue(string.Format("@Code{0}", i), data.orderItems[i].code);
                    //    cmd.Parameters.AddWithValue(string.Format("@Quantity{0}", i), data.orderItems[i].quantity);
                    //}

                    var dateTimeNowOptimizeString = "";
                    if (_configuration.GetValue<bool>("optimizeDateTimeNowEveryHour"))
                    {
                        dateTimeNowOptimizeString = DateMove.ToString("yyyy-MM-ddTHH:00:00");
                    }
                    else
                    {
                        dateTimeNowOptimizeString = DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss");
                    }

                    cmd.CommandText = queryTextBegin + string.Format(query, "",
                        dateTimeNowOptimizeString,
                        DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                        DateMove.Date.AddDays(Parameters1C.First(x => x.Name.Contains("rsp_КоличествоДнейЗаполненияГрафика")).ValueDouble - 1).ToString("yyyy-MM-ddTHH:mm:ss"),
                        Parameters1C.First(x => x.Name.Contains("КоличествоДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа")).ValueDouble,
                        Parameters1C.First(x => x.Name.Contains("ПроцентДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа")).ValueDouble);

                    

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

                            result.data.Add(new ResponseIntervalListElement
                            {
                                begin = begin,
                                end = end
                            });

                            resultLog.data.Add(new ResponseIntervalListElementWithOffSet
                            {
                                begin = new(begin),
                                end = new(end)
                            });
                        }
                    }

                    var stats = conn.RetrieveStatistics();
                    sqlCommandExecutionTime = (long)stats["ExecutionTime"];

                    //close data reader
                    dr.Close();

                    //close connection
                    conn.Close();

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


            watch.Stop();
            logElement.TimeSQLExecutionFact = watch.ElapsedMilliseconds;

            stopwatchExecution.Stop();
            logElement.TimeFullExecution = stopwatchExecution.ElapsedMilliseconds;

            var logstringElement = JsonSerializer.Serialize(logElement);

            _logger.LogInformation(logstringElement);

            return Ok(result);
        }

        private static void FillGoodsTable(RequestIntervalList data, SqlConnection conn)
        {
            RequestDataAvailableDate convertedData = new()
            {
                codes = data.orderItems.ToArray()
            };

            FillGoodsTable(convertedData, conn);
        }


        private static void FillGoodsTable(RequestDataAvailableDate data, SqlConnection conn)
        {

            string queryGoodsCreate = Queries.CreateTableGoodsRawCreate;

            SqlCommand cmdGoodsTableCreate = new(queryGoodsCreate, conn);

            var GoodsRowsCreate = cmdGoodsTableCreate.ExecuteNonQuery();

            string queryGoods = Queries.CreateTableGoodsRawInsert;

            SqlCommand cmdGoodsTable = new(queryGoods, conn);

            var parameters = new List<string>();//string[data.codes.Length];

            var codesCounter = 0;
            var pickupPointsCounter = 0;

            foreach (var codesElem in data.codes)
            {

                var parameterString = string.Format("(@Article{0}, @Code{0}, NULL, @Quantity{0})", codesCounter);

                cmdGoodsTable.Parameters.AddWithValue(string.Format("@Article{0}", codesCounter), codesElem.article);
                if (String.IsNullOrEmpty(codesElem.code))
                    cmdGoodsTable.Parameters.AddWithValue(string.Format("@Code{0}", codesCounter), DBNull.Value);
                else
                    cmdGoodsTable.Parameters.AddWithValue(string.Format("@Code{0}", codesCounter), codesElem.code);
                cmdGoodsTable.Parameters.AddWithValue(string.Format("@Quantity{0}", codesCounter), codesElem.quantity);

                parameters.Add(parameterString);

                foreach (var pickupElem in codesElem.PickupPoints)
                {
                    var parameterStringPickup = string.Format("(@Article{0}, @Code{0}, @PickupPoint{1}, @Quantity{0})", codesCounter, pickupPointsCounter);

                    cmdGoodsTable.Parameters.AddWithValue(string.Format("@PickupPoint{0}", pickupPointsCounter), pickupElem);

                    parameters.Add(parameterStringPickup);

                    pickupPointsCounter++;
                }
                codesCounter++;
            }

            cmdGoodsTable.CommandText = string.Format(queryGoods, string.Join(", ", parameters));

            var GoodsRows = cmdGoodsTable.ExecuteNonQuery();
        }

        private static DataTable FillGoodsTableParameter(RequestDataAvailableDate data)
        {

            var table = new DataTable();
            table.Columns.Add("Article", typeof(string));
            table.Columns.Add("code", typeof(string));
            table.Columns.Add("PickupPoint", typeof(string));
            table.Columns.Add("quantity", typeof(string));

            foreach (var codesElem in data.codes)
            {


                var row = table.NewRow();

                row["Article"] = codesElem.article;

                if (String.IsNullOrEmpty(codesElem.code))
                    row["code"] = DBNull.Value;
                else
                    row["code"] = codesElem.code;


                row["PickupPoint"] = DBNull.Value;
                row["Quantity"] = codesElem.quantity;

                table.Rows.Add(row);

               
                foreach (var pickupElem in codesElem.PickupPoints)
                {
                    var rowPickup = table.NewRow();

                    rowPickup["Article"] = codesElem.article;

                    if (String.IsNullOrEmpty(codesElem.code))
                        rowPickup["code"] = DBNull.Value;
                    else
                        rowPickup["code"] = codesElem.code;


                    rowPickup["PickupPoint"] = pickupElem;
                    rowPickup["Quantity"] = codesElem.quantity;

                    table.Rows.Add(rowPickup);
                }
                
            }

            return table;
        }


        private static string TextFillGoodsTable(RequestIntervalList data, SqlCommand cmdGoodsTable, bool optimizeRowsCount)
        {
            RequestDataAvailableDate convertedData = new()
            {
                codes = data.orderItems.ToArray()
            };

            return TextFillGoodsTable(convertedData, cmdGoodsTable, optimizeRowsCount);
        }

        private static string TextFillGoodsTable(RequestDataAvailableDate data, SqlCommand cmdGoodsTable, bool optimizeRowsCount)
        {

          
            string queryGoods = Queries.CreateTableGoodsRawCreate + Queries.CreateTableGoodsRawInsert;

            var parameters = new List<string>();//string[data.codes.Length];

            //var codesCounter = 0;
            //var pickupPointsCounter = 0;

            var maxPickups = 0;
            List<string> PickupsList = new();

            var maxCodes = data.codes.Length;

            foreach (var codesElem in data.codes)
            {
                foreach (var item in codesElem.PickupPoints)
                {
                    if (!PickupsList.Contains(item))
                    {
                        PickupsList.Add(item);
                    }                    
                }
            }

            maxPickups = PickupsList.Count;
            PickupsList.Add("");
            //if (maxPickups > 2) maxPickups = 7;

            if (data.codes.Length > 2) maxCodes = 10;
            if (data.codes.Length > 10) maxCodes = 30;
            if (data.codes.Length > 30) maxCodes = 60;
            if (data.codes.Length > 60) maxCodes = 100;
            if (data.codes.Length > maxCodes || !optimizeRowsCount) maxCodes = data.codes.Length;

            foreach (var pickupElem in PickupsList)
            {
                var index = PickupsList.IndexOf(pickupElem);
                cmdGoodsTable.Parameters.Add(string.Format("@PickupPoint{0}", index), SqlDbType.NVarChar, 4);
                cmdGoodsTable.Parameters[string.Format("@PickupPoint{0}", index)].Value = pickupElem;

                if (String.IsNullOrEmpty(pickupElem))
                {
                    cmdGoodsTable.Parameters[string.Format("@PickupPoint{0}", index)].Value = DBNull.Value;
                }
            }

            for (int codesCounter = 0; codesCounter < maxCodes; codesCounter++)
            {

                RequestDataCodeItem codesElem;
                if (codesCounter< data.codes.Length)
                {
                    codesElem = data.codes[codesCounter];
                }
                else
                {
                    codesElem = data.codes[data.codes.Length-1];
                }

                var pickupPointsCounter = 0;
                var indexNullPickup = PickupsList.IndexOf("");
                var parameterString = string.Format("(@Article{0}, @Code{0}, @PickupPoint{1}, @Quantity{0})", codesCounter, indexNullPickup);

                
                //cmdGoodsTable.Parameters.AddWithValue(string.Format("@Article{0}", codesCounter), codesElem.article);
                cmdGoodsTable.Parameters.Add(string.Format("@Article{0}", codesCounter), SqlDbType.NVarChar, 11);
                cmdGoodsTable.Parameters[string.Format("@Article{0}", codesCounter)].Value = codesElem.article;

                cmdGoodsTable.Parameters.Add(string.Format("@Code{0}", codesCounter), SqlDbType.NVarChar, 11);
                if (String.IsNullOrEmpty(codesElem.code))
                    cmdGoodsTable.Parameters[string.Format("@Code{0}", codesCounter)].Value = DBNull.Value;
                else
                    cmdGoodsTable.Parameters[string.Format("@Code{0}", codesCounter)].Value = codesElem.code;

                //cmdGoodsTable.Parameters.AddWithValue(string.Format("@Quantity{0}", codesCounter), codesElem.quantity);
                cmdGoodsTable.Parameters.Add(string.Format("@Quantity{0}", codesCounter), SqlDbType.Int, 10);
                cmdGoodsTable.Parameters[string.Format("@Quantity{0}", codesCounter)].Value = codesElem.quantity;

                parameters.Add(parameterString);

                foreach (var pickupElem in codesElem.PickupPoints)
                {

                    var index = PickupsList.IndexOf(pickupElem);
                    var parameterStringPickup = string.Format("(@Article{0}, @Code{0}, @PickupPoint{1}, @Quantity{0})", codesCounter, index);                    

                    parameters.Add(parameterStringPickup);

                    pickupPointsCounter++;
                }

                while (pickupPointsCounter < maxPickups)
                {

                    var index = PickupsList.IndexOf("");
                    var parameterStringPickup = string.Format("(@Article{0}, @Code{0}, @PickupPoint{1}, @Quantity{0})", codesCounter, index);

                    //cmdGoodsTable.Parameters.AddWithValue(string.Format("@PickupPoint{0}", pickupPointsCounter), DBNull.Value);

                    //cmdGoodsTable.Parameters.Add(string.Format("@PickupPoint{0}", pickupPointsCounter), SqlDbType.NVarChar, 4);
                    //cmdGoodsTable.Parameters[string.Format("@PickupPoint{0}", pickupPointsCounter)].Value = DBNull.Value;

                    parameters.Add(parameterStringPickup);

                    pickupPointsCounter++;
                }                

                //codesCounter++;
            }

            return string.Format(queryGoods, string.Join(", ", parameters));

        }

    }
}

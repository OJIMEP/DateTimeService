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

        public DateTimeController(ILogger<DateTimeController> logger, IConfiguration configuration, ILoadBalancing loadBalancing, IGeoZones geoZones)
        {
            _logger = logger;
            _configuration = configuration;
            _loadBalacing = loadBalancing;
            _geoZones = geoZones;
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
        public async Task<IActionResult> AvailableDateAsync(RequestDataAvailableDate data)
        {

            Stopwatch stopwatchExecution = new();
            stopwatchExecution.Start();


            //string connString = _configuration.GetConnectionString("1CDataSqlConnection");
            Stopwatch watch = new();
            watch.Start();
            string connString = await _loadBalacing.GetDatabaseConnectionAsync();
            watch.Stop();
            //string connString = @"Server=localhost;Database=DevBase_cut_v3;Uid=sa;Pwd=; Trusted_Connection = False;";

            ResponseAvailableDate result = new();


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
                //sql connection object
                using SqlConnection conn = new(connString);

                conn.StatisticsEnabled = true;

                string query = Queries.AvailableDate;


                var DateMove = DateTime.Now.AddMonths(24000);
                var TimeNow = new DateTime(2001, 1, 1, DateMove.Hour, DateMove.Minute, DateMove.Second);
                var EmptyDate = new DateTime(2001, 1, 1, 0, 0, 0);
                var MaxDate = new DateTime(5999, 11, 11, 0, 0, 0);

                //define the SqlCommand object
                SqlCommand cmd = new(query, conn);

                cmd.Parameters.Add("@P4", SqlDbType.NVarChar);
                cmd.Parameters["@P4"].Value = data.city_id;

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


                
                var count = data.codes.Length > 60 ? data.codes.Length : 60;
                var parameters = new string[count];

                for (int i = 0; i < count; i++)
                {
                    parameters[i] = string.Format("@Article{0}", i);
                    cmd.Parameters.Add(parameters[i], SqlDbType.NVarChar,10);

                    if (i >= data.codes.Length)
                    {
                        cmd.Parameters[parameters[i]].Value = data.codes[0];
                    }
                    else
                    {
                        cmd.Parameters[parameters[i]].Value = data.codes[i];
                    }

                    
                    //cmd.Parameters.AddWithValue(parameters[i], data.codes[i]);
                }

                cmd.CommandText = string.Format(query, string.Join(", ", parameters),
                    DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                    DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                    DateMove.Date.AddDays(Parameters1C.First(x => x.Name.Contains("rsp_КоличествоДнейЗаполненияГрафика")).ValueDouble - 1).ToString("yyyy-MM-ddTHH:mm:ss"),
                    Parameters1C.First(x => x.Name.Contains("КоличествоДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа")).ValueDouble,
                    Parameters1C.First(x => x.Name.Contains("ПроцентДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа")).ValueDouble);

                //open connection
                conn.Open();

                //execute the SQLCommand
                SqlDataReader dr = cmd.ExecuteReader();

                //check if there are records
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        var article = dr.GetString(0);
                        var availableDateCourier = dr.GetDateTime(1).AddMonths(-24000);
                        var availableDateSelf = dr.GetDateTime(2).AddMonths(-24000);

                        result.code.Add(article);
                        result.courier.Add(availableDateCourier);
                        result.self.Add(availableDateSelf);
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

            var resultDict = new ResponseAvailableDateDict();
            for (int i = 0; i < result.code.Count; i++)
            {
                var resEl = new ResponseAvailableDateDictElement
                {
                    code = result.code[i],
                    courier = data.delivery_types.Contains("courier") ? result.courier[i].Date.ToString("yyyy-MM-ddTHH:mm:ss") : null,
                    self = data.delivery_types.Contains("self") ? result.self[i].Date.ToString("yyyy-MM-ddTHH:mm:ss") : null
                };

                resultDict.data.Add(result.code[i], resEl);
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
        public async Task<IActionResult> IntervalListAsync(RequestIntervalList data)
        {

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
                RequestContent = JsonSerializer.Serialize(data),
                Id = Guid.NewGuid().ToString(),
                DatabaseConnection = LoadBalancing.RemoveCredentialsFromConnectionString(connString),
                AuthenticatedUser = User.Identity.Name,
                LoadBalancingExecution = watch.ElapsedMilliseconds
            };

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
            watch.Start();

            string zoneId = "";

            bool alwaysCheckGeozone = _configuration.GetValue<bool>("alwaysCheckGeozone");

            bool adressExists = _geoZones.AdressExists(connString, data.address_id);

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

                    string query = Queries.IntervalList;


                    var DateMove = DateTime.Now.AddMonths(24000);
                    var TimeNow = new DateTime(2001, 1, 1, DateMove.Hour, DateMove.Minute, DateMove.Second);
                    var EmptyDate = new DateTime(2001, 1, 1, 0, 0, 0);
                    var MaxDate = new DateTime(5999, 11, 11, 0, 0, 0);

                    //define the SqlCommand object
                    SqlCommand cmd = new(query, conn);

                    cmd.Parameters.Add("@P4", SqlDbType.NVarChar);
                    cmd.Parameters["@P4"].Value = data.address_id;

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
                    cmd.Parameters["@P_DateTimePeriodEnd"].Value = DateMove.Date.AddDays(Parameters1C.First(x => x.Name.Contains("rsp_КоличествоДнейЗаполненияГрафика")).ValueDouble-1);

                    cmd.Parameters.Add("@P_TimeNow", SqlDbType.DateTime);
                    cmd.Parameters["@P_TimeNow"].Value = TimeNow;

                    cmd.Parameters.Add("@P_EmptyDate", SqlDbType.DateTime);
                    cmd.Parameters["@P_EmptyDate"].Value = EmptyDate;

                    cmd.Parameters.Add("@P_MaxDate", SqlDbType.DateTime);
                    cmd.Parameters["@P_MaxDate"].Value = MaxDate;

                    cmd.Parameters.Add("@P_GeoCode", SqlDbType.NVarChar);
                    cmd.Parameters["@P_GeoCode"].Value = zoneId;



                    var parameters = new string[data.orderItems.Count];
                    for (int i = 0; i < data.orderItems.Count; i++)
                    {
                        parameters[i] = string.Format("(@Article{0},@Quantity{0})", i);

                        cmd.Parameters.AddWithValue(string.Format("@Article{0}", i), data.orderItems[i].code);
                        cmd.Parameters.AddWithValue(string.Format("@Quantity{0}", i), data.orderItems[i].quantity);
                    }

                    cmd.CommandText = string.Format(query, string.Join(", ", parameters),
                        DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                        DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                        DateMove.Date.AddDays(Parameters1C.First(x => x.Name.Contains("rsp_КоличествоДнейЗаполненияГрафика")).ValueDouble-1).ToString("yyyy-MM-ddTHH:mm:ss"),
                        Parameters1C.First(x => x.Name.Contains("КоличествоДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа")).ValueDouble,
                        Parameters1C.First(x => x.Name.Contains("ПроцентДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа")).ValueDouble);

                    //open connection
                    conn.Open();

                    //execute the SQLCommand
                    SqlDataReader dr = cmd.ExecuteReader();

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


            watch.Stop();
            logElement.TimeSQLExecutionFact = watch.ElapsedMilliseconds;

            stopwatchExecution.Stop();
            logElement.TimeFullExecution = stopwatchExecution.ElapsedMilliseconds;

            var logstringElement = JsonSerializer.Serialize(logElement);

            _logger.LogInformation(logstringElement);

            return Ok(result);
        }

    }
}

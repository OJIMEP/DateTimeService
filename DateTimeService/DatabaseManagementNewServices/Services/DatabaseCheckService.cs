using DateTimeService.DatabaseManagementNewServices.Interfaces;
using DateTimeService.Models;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using DateTimeService.Data;
using DateTimeService.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Text.Json;
using System.Text;
using DateTimeService.DatabaseManagementUtils;
using System.Net.Http;
using System.Net;
using System.Net.Sockets;

namespace DateTimeService.DatabaseManagementNewServices.Services
{
    public class DatabaseCheckService : IDatabaseCheck
    {
        private readonly ILogger<DatabaseCheckService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly bool _productionEnv;

        public DatabaseCheckService(ILogger<DatabaseCheckService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;

            _productionEnv = _configuration["loggerEnv"] == "Production";
        }

        public async Task<bool> CheckAggregationsAsync(string databaseConnectionString, CancellationToken cancellationToken)
        {

            int result = -1;

            Stopwatch watch = new();
            watch.Start();
            try
            {
                using SqlConnection conn = new(databaseConnectionString);

                //open connection
                conn.Open();

                string query = Queries.CheckAggregations;

                SqlCommand cmd = new(query, conn);

                cmd.CommandTimeout = 20;
                cmd.CommandText = query;

                //execute the SQLCommand
                var dataReader = await cmd.ExecuteReaderAsync(cancellationToken);
                if (dataReader.HasRows)
                {
                    if (dataReader.Read())
                    {

                        if (dataReader.GetInt32(0) == 0)
                        {
                            result = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = -1;

                var logElement = new ElasticLogElement
                {
                    LoadBalancingExecution = 0,
                    ErrorDescription = ex.Message,
                    Status = LogStatus.Error,
                    DatabaseConnection = LoadBalancing.RemoveCredentialsFromConnectionString(databaseConnectionString)
                };

                var logstringElement = JsonSerializer.Serialize(logElement);

                _logger.LogInformation(logstringElement);

            }
            watch.Stop();

            return result >= 0;
        }

        public async Task<bool> CheckAvailabilityAsync(string databaseConnectionString, CancellationToken cancellationToken, long executionLimit = 5000)
        {
            int result;

            Stopwatch watch = new();
            watch.Start();
            try
            {
                using SqlConnection conn = new(databaseConnectionString);

                //open connection
                conn.Open();

                List<string> queryParts = new()
                {
                    Queries.AvailableDate1,
                    Queries.AvailableDate2MinimumWarehousesBasic,
                    Queries.AvailableDate3,
                    Queries.AvailableDate4SourcesWithPrices,
                    Queries.AvailableDate5,
                    Queries.AvailableDate6IntervalsBasic,
                    Queries.AvailableDate7,
                    Queries.AvailableDate8DeliveryPowerBasic,
                    Queries.AvailableDate9
                };

                string query = String.Join("", queryParts);

                var DateMove = DateTime.Now.AddMonths(24000);
                var TimeNow = new DateTime(2001, 1, 1, DateMove.Hour, DateMove.Minute, DateMove.Second);
                var EmptyDate = new DateTime(2001, 1, 1, 0, 0, 0);
                var MaxDate = new DateTime(5999, 11, 11, 0, 0, 0);

                SqlCommand cmd = new(query, conn);

                List<string> pickups = new();

                var data = new RequestDataAvailableDate(true);

                var queryTextBegin = DateTimeController.TextFillGoodsTable(data, cmd, true, pickups);

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

                var Parameters1C = new List<GlobalParam1C>
                {
                    new GlobalParam1C
                    {
                        Name = "rsp_КоличествоДнейЗаполненияГрафика",
                        ValueDouble = 5,
                    },
                    new GlobalParam1C
                    {
                        Name = "КоличествоДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа",
                        ValueDouble = 4
                    },
                    new GlobalParam1C
                    {
                        Name = "ПроцентДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа",
                        ValueDouble = 3
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

                cmd.Parameters.Add("@P_ApplyShifting", SqlDbType.Int);
                cmd.Parameters["@P_ApplyShifting"].Value = Parameters1C.First(x => x.Name.Contains("ПрименятьСмещениеДоступностиПрослеживаемыхМаркируемыхТоваров")).ValueDouble;

                cmd.Parameters.Add("@P_DaysToShift", SqlDbType.Int);
                cmd.Parameters["@P_DaysToShift"].Value = Parameters1C.First(x => x.Name.Contains("КоличествоДнейСмещенияДоступностиПрослеживаемыхМаркируемыхТоваров")).ValueDouble;

                cmd.CommandTimeout = (int)(executionLimit / 1000);


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



                cmd.CommandText = queryTextBegin + string.Format(query, string.Join(",", pickupParameters),
                    dateTimeNowOptimizeString,
                    DateMove.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                    DateMove.Date.AddDays(Parameters1C.First(x => x.Name.Contains("rsp_КоличествоДнейЗаполненияГрафика")).ValueDouble - 1).ToString("yyyy-MM-ddTHH:mm:ss"),
                    Parameters1C.First(x => x.Name.Contains("КоличествоДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа")).ValueDouble,
                    Parameters1C.First(x => x.Name.Contains("ПроцентДнейАнализаЛучшейЦеныПриОтсрочкеЗаказа")).ValueDouble,
                    pickupWorkingHoursJoinType,
                    "");


                //execute the SQLCommand
                result = await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                result = -1;

                var logElement = new ElasticLogElement
                {
                    LoadBalancingExecution = 0,
                    ErrorDescription = ex.Message,
                    Status = LogStatus.Error,
                    DatabaseConnection = LoadBalancing.RemoveCredentialsFromConnectionString(databaseConnectionString)
                };

                var logstringElement = JsonSerializer.Serialize(logElement);

                _logger.LogInformation(logstringElement);

            }
            watch.Stop();

            if (watch.ElapsedMilliseconds > executionLimit)
            {
                result = -1;
                var logElement = new ElasticLogElement
                {
                    LoadBalancingExecution = 0,
                    ErrorDescription = "Availability false because of ElapsedMilliseconds=" + watch.ElapsedMilliseconds,
                    Status = LogStatus.Error,
                    DatabaseConnection = LoadBalancing.RemoveCredentialsFromConnectionString(databaseConnectionString)
                };

                var logstringElement = JsonSerializer.Serialize(logElement);

                _logger.LogInformation(logstringElement);
            }

            

            return result >= 0;
        }

        public async Task<ElasticDatabaseStats> GetElasticLogsInformationAsync(string databaseConnectionWithOutCredentials, CancellationToken cancellationToken)
        {
            if (!_configuration.GetValue<bool>("UseLoadBalance2"))
            {
                return null;
            }

            var elasticHost = _configuration["elasticsearch:host"];
            var elasticPort = _configuration["elasticsearch:port"];
            var elasticLogin = _configuration["elasticsearch:login"];
            var elasticPass = _configuration["elasticsearch:password"];
            var indexPath = _configuration["elasticsearch:indexName"];
            var authenticationString = elasticLogin + ":" + elasticPass;
            var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString));
            var analyzeInterval = "now-1m";
            var clearCacheCriterias = _configuration.GetSection("ClearCacheCriterias").Get<List<ClearCacheCriteria>>();

            ElasticResponse elasticResponse = null;

            var httpClient = _httpClientFactory.CreateClient("elastic");

            UriBuilder elasticUri = new("https", elasticHost, int.Parse(elasticPort), indexPath + "/_search");

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var serverIP = ipHostInfo.AddressList.Where(s => s.AddressFamily == AddressFamily.InterNetwork).First().ToString();
            HttpRequestMessage requestMessage = new(HttpMethod.Get, elasticUri.Uri);
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

            var searchrequest = new ElasticRequest
            {
                Size = 0
            };

            FilterElement element = new();
            element.Range = new();
            element.Range.Add("@timestamp", new { gt = analyzeInterval });

            searchrequest.Query.Bool.Filter.Add(element);

            element = new();
            element.Term = new();
            element.Term.Add("server_host.keyword", new { value = serverIP });
            searchrequest.Query.Bool.Filter.Add(element);

            element = new();
            element.Term = new();
            element.Term.Add("DatabaseConnection.keyword", new { value = databaseConnectionWithOutCredentials });
            searchrequest.Query.Bool.Filter.Add(element);


            AggregationClass rootAgg = new();
            rootAgg.Terms = new();
            rootAgg.Terms.Field = "DatabaseConnection.keyword";
            rootAgg.Terms.Size = 5;

            rootAgg.Aggregations = new();

            AggregationClass timePercentile = new();
            timePercentile.Percentiles = new();
            timePercentile.Percentiles.Field = "TimeFullExecution";
            timePercentile.Percentiles.Percents = new double[] { 95, 99, 99.5 };

            rootAgg.Aggregations.Add("time_percentile", timePercentile);

            AggregationClass loadBal = new();
            loadBal.Avg = new();
            loadBal.Avg.Field = "LoadBalancingExecution";
            rootAgg.Aggregations.Add("load_bal", loadBal);

            AggregationClass average = new();
            average.Avg = new();
            average.Avg.Field = "TimeFullExecution";
            rootAgg.Aggregations.Add("week_avg", average);

            searchrequest.Aggregations.Add("load_time_outlier", rootAgg);

            var content = JsonSerializer.Serialize(searchrequest);
            requestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");


            var result = "";

            try
            {
                var response = await httpClient.SendAsync(requestMessage, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsStringAsync(cancellationToken);
                }
                else
                {
                    var errodData = await response.Content.ReadAsStringAsync(cancellationToken);
                    var logElement = new ElasticLogElement
                    {
                        ErrorDescription = errodData,
                        Status = LogStatus.Error,
                        DatabaseConnection = elasticUri.ToString()
                    };

                    logElement.AdditionalData.Add("requestContent", content);
                    var logstringElement = JsonSerializer.Serialize(logElement);

                    _logger.LogError(logstringElement);
                }


            }
            catch (Exception ex)
            {
                var logElement = new ElasticLogElement
                {
                    TimeSQLExecution = 0,
                    ErrorDescription = ex.Message,
                    Status = LogStatus.Error,
                    DatabaseConnection = elasticUri.ToString()
                };

                var logstringElement = JsonSerializer.Serialize(logElement);

                _logger.LogError(logstringElement);
            }


            try
            {
                if (!String.IsNullOrEmpty(result))
                {
                    elasticResponse = JsonSerializer.Deserialize<ElasticResponse>(result);
                }
            }
            catch (Exception ex)
            {
                var logElement = new ElasticLogElement
                {
                    TimeSQLExecution = 0,
                    ErrorDescription = ex.Message,
                    Status = LogStatus.Error,
                    DatabaseConnection = elasticUri.ToString()
                };
                logElement.AdditionalData.Add("responseContent", result);
                var logstringElement = JsonSerializer.Serialize(logElement);

                _logger.LogError(logstringElement);
            }

            if (elasticResponse == null)
            {
                return null;
            }

            if (!elasticResponse.Aggregations.TryGetValue("load_time_outlier", out Aggregations aggregation))
            {
                _logger.LogError("Aggregation load_time_outlier not found in Elastic response");
                return null;
            }

            var responseBucket = aggregation.Buckets.Find(x => x.Key == databaseConnectionWithOutCredentials);
            if (responseBucket == null)
            {
                if (_productionEnv)
                {
                    _logger.LogError($"Database with key {databaseConnectionWithOutCredentials} has no logs in Elastic!");
                }                
                return null;
            }

            var databaseStats = new ElasticDatabaseStats
            {
                RecordCount = responseBucket.DocCount,
                LoadBalanceTime = responseBucket.LoadBalance.Value,
                AverageTime = responseBucket.WeekAvg.Value,
                Percentile95Time = responseBucket.TimePercentile.Values.GetValueOrDefault("95.0")
            };

            if (databaseStats.LoadBalanceTime == default || databaseStats.AverageTime == default || databaseStats.Percentile95Time == default)
            {
                if (_productionEnv)
                {
                    _logger.LogError($"db stats for {databaseConnectionWithOutCredentials} is empty");
                }
                
                return null;
            }

            return databaseStats;
        }
    }
}

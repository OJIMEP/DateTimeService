﻿using DateTimeService.Controllers;
using DateTimeService.DatabaseManagementUtils;
using DateTimeService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DateTimeService.Data
{
    public class DatabaseManagement
    {

        private readonly ILogger<DateTimeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public DatabaseManagement(ILogger<DateTimeController> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task CheckDatabaseStatus(CancellationToken cancellationToken)
        {

            if (!_configuration.GetValue<bool>("UseLoadBalance2"))
            {
                return;
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

            if (elasticResponse != null)
            {
                foreach (var rootAggregation in elasticResponse.Aggregations)
                {
                    if (rootAggregation.Key == "load_time_outlier")
                    {
                        foreach (var bucket in rootAggregation.Value.Buckets)
                        {
                            var minutesInBucket = int.Parse(analyzeInterval.Replace("now-", "").Replace("m", ""));
                            var recordsByMinute = bucket.DocCount / minutesInBucket;
                            var database = DatabaseList.Databases.FirstOrDefault(s => s.ConnectionWithoutCredentials == bucket.Key);
                            var criteria = clearCacheCriterias.FirstOrDefault(s => recordsByMinute >= s.RecordCountBegin && recordsByMinute <= s.RecordCountEnd && s.CriteriaType == "RecordCount");
                            var criteriaMaxTime = clearCacheCriterias.FirstOrDefault(s => s.CriteriaType == "MaximumResponseTime");
                            var percentile95rate = bucket.TimePercentile.Values.GetValueOrDefault("95.0");

                            var DelayBetweenClearCache = _configuration.GetValue<int>("DelayBetweenClearCache");
                            if (DelayBetweenClearCache == 0)
                            {
                                DelayBetweenClearCache = 180;
                            }

                            var ErrorsCountToSendClearCache = _configuration.GetValue<int>("ErrorsCountToSendClearCache");
                            if (ErrorsCountToSendClearCache == 0)
                            {
                                ErrorsCountToSendClearCache = 1;
                            }

                            var checkAvailability = false;

                            if (database == default || criteria == default || percentile95rate == default)
                            {
                                continue;
                            }


                            if (percentile95rate > criteria.Percentile_95
                                && database.Type != "main"
                                && (database.LastFreeProcCacheCommand == default || DateTimeOffset.Now - database.LastFreeProcCacheCommand > TimeSpan.FromSeconds(DelayBetweenClearCache)))
                            {
                                if (database.TimeCriteriaFailCount >= ErrorsCountToSendClearCache)
                                {
                                    try
                                    {
                                        using SqlConnection conn = new(database.Connection);

                                        conn.Open();

                                        var clearCacheScript = Queries.ClearCacheScriptDefault;

                                        var clearCacheScriptFromConfig = _configuration.GetValue<string>("ClearCacheScript");

                                        if (!String.IsNullOrEmpty(clearCacheScriptFromConfig))
                                        {
                                            clearCacheScript = clearCacheScriptFromConfig;
                                        }

                                        SqlCommand cmd = new(clearCacheScript, conn);

                                        cmd.CommandTimeout = 1;

                                        var clearCacheResult = await cmd.ExecuteNonQueryAsync(cancellationToken);
                                        conn.Close();

                                        database.LastFreeProcCacheCommand = DateTimeOffset.Now;

                                        var logElement = new ElasticLogElement
                                        {
                                            LoadBalancingExecution = 0,
                                            ErrorDescription = "Send dbcc freeproccache",
                                            Status = LogStatus.Ok,
                                            DatabaseConnection = database.ConnectionWithoutCredentials
                                        };

                                        var logstringElement = JsonSerializer.Serialize(logElement);

                                        _logger.LogInformation(logstringElement);
                                    }
                                    catch (Exception ex)
                                    {
                                        var logElement = new ElasticLogElement
                                        {
                                            LoadBalancingExecution = 0,
                                            ErrorDescription = ex.Message,
                                            Status = LogStatus.Error,
                                            DatabaseConnection = database.ConnectionWithoutCredentials
                                        };

                                        var logstringElement = JsonSerializer.Serialize(logElement);

                                        _logger.LogError(logstringElement);

                                        checkAvailability = true;
                                    }
                                }
                                else
                                {
                                    database.TimeCriteriaFailCount++;
                                }
                            }
                            else
                            {
                                database.TimeCriteriaFailCount = 0;
                            }

                            if (checkAvailability)
                            {
                                var checkResult = await CheckDatabaseAvailability(database.Connection, cancellationToken, (int)criteriaMaxTime.Percentile_95);

                                database.AvailableToUse = checkResult;

                                if (database.Type == "replica_tables")
                                {
                                    var aggResult = await CheckAggregationsAvailability(database.Connection, cancellationToken);

                                    database.CustomAggregationsAvailable = aggResult;
                                }
                                else
                                    database.CustomAggregationsAvailable = false;




                                var logElement = new ElasticLogElement
                                {
                                    LoadBalancingExecution = 0,
                                    ErrorDescription = "Checked availability",
                                    Status = LogStatus.Ok,
                                    DatabaseConnection = database.ConnectionWithoutCredentials
                                };
                                logElement.AdditionalData.Add("Available", checkResult.ToString());
                                logElement.AdditionalData.Add("AvailableAggs", database.CustomAggregationsAvailable.ToString());
                                var logstringElement = JsonSerializer.Serialize(logElement);

                                _logger.LogInformation(logstringElement);
                            }

                            if (recordsByMinute >= 100 && bucket.WeekAvg.Value == 0)
                            {
                                database.AvailableToUse = false;

                                var logElement = new ElasticLogElement
                                {
                                    LoadBalancingExecution = 0,
                                    ErrorDescription = "Database disabled due to zero response time",
                                    Status = LogStatus.Error,
                                    DatabaseConnection = database.ConnectionWithoutCredentials
                                };

                                var logstringElement = JsonSerializer.Serialize(logElement);

                                _logger.LogInformation(logstringElement);
                            }

                            if (recordsByMinute >= 100 && bucket.LoadBalance.Value > criteriaMaxTime.LoadBalance)
                            {
                                database.AvailableToUse = false;

                                var logElement = new ElasticLogElement
                                {
                                    LoadBalancingExecution = 0,
                                    ErrorDescription = "Database disabled due to big load balance time",
                                    Status = LogStatus.Error,
                                    DatabaseConnection = database.ConnectionWithoutCredentials
                                };

                                var logstringElement = JsonSerializer.Serialize(logElement);

                                _logger.LogInformation(logstringElement);
                            }

                            if (recordsByMinute >= 100 && bucket.WeekAvg.Value > criteriaMaxTime.Percentile_95)
                            {
                                database.AvailableToUse = false;

                                var logElement = new ElasticLogElement
                                {
                                    LoadBalancingExecution = 0,
                                    ErrorDescription = "Database disabled due to big execution time",
                                    Status = LogStatus.Error,
                                    DatabaseConnection = database.ConnectionWithoutCredentials
                                };

                                var logstringElement = JsonSerializer.Serialize(logElement);

                                _logger.LogInformation(logstringElement);
                            }
                            database.LastCheckAvailability = DateTimeOffset.Now;
                            database.LastCheckPerfomance = DateTimeOffset.Now;
                        }
                    }
                }
            }

            foreach (var item in DatabaseList.Databases)
            {
                if (!item.AvailableToUse && DateTimeOffset.Now - item.LastCheckAvailability > TimeSpan.FromSeconds(60))
                {

                    var checkResult = await CheckDatabaseAvailability(item.Connection, cancellationToken);

                    item.AvailableToUse = checkResult;

                    //if (item.Type == "replica_tables")
                    //{
                    //    var aggResult = await CheckAggregationsAvailability(item.Connection, cancellationToken);

                    //    item.CustomAggregationsAvailable = aggResult;
                    //}
                    //else
                    //    item.CustomAggregationsAvailable = false;



                    item.LastCheckAvailability = DateTimeOffset.Now;

                    var logElement = new ElasticLogElement
                    {
                        LoadBalancingExecution = 0,
                        ErrorDescription = "Checked availability",
                        Status = LogStatus.Ok,
                        DatabaseConnection = item.ConnectionWithoutCredentials
                    };
                    logElement.AdditionalData.Add("Available", checkResult.ToString());
                    logElement.AdditionalData.Add("AvailableAggs", item.CustomAggregationsAvailable.ToString());
                    var logstringElement = JsonSerializer.Serialize(logElement);

                    _logger.LogInformation(logstringElement);

                }

                if (item.AvailableToUse && DateTimeOffset.Now - item.LastCheckAggregations > TimeSpan.FromSeconds(60))
                {

                    item.LastCheckAggregations = DateTimeOffset.Now;

                    var oldAggs = item.CustomAggregationsAvailable;

                    if (item.Type == "replica_tables" && item.AvailableToUse)
                    {
                        var aggResult = await CheckAggregationsAvailability(item.Connection, cancellationToken);

                        if (aggResult == true)
                        {
                            item.CustomAggsFailCount = 0;
                            item.CustomAggregationsAvailable = true;
                        }
                        else
                        {
                            item.CustomAggsFailCount++;

                            if (item.CustomAggsFailCount >= 6)
                            {
                                item.CustomAggregationsAvailable = false;
                            }
                        }

                        //item.CustomAggregationsAvailable = aggResult;
                    }
                    else
                        item.CustomAggregationsAvailable = false;

                    if (item.Type == "replica_tables" && item.AvailableToUse && oldAggs != item.CustomAggregationsAvailable)
                    {
                        var logElement = new ElasticLogElement
                        {
                            LoadBalancingExecution = 0,
                            ErrorDescription = item.CustomAggregationsAvailable ? "Enabled aggregations" : "Disabled aggregations",
                            Status = LogStatus.Ok,
                            DatabaseConnection = item.ConnectionWithoutCredentials
                        };
                        logElement.AdditionalData.Add("Available", item.AvailableToUse.ToString());
                        logElement.AdditionalData.Add("AvailableAggs", item.CustomAggregationsAvailable.ToString());
                        var logstringElement = JsonSerializer.Serialize(logElement);

                        _logger.LogInformation(logstringElement);
                    }
                }

            }

        }

        public async Task<bool> CheckDatabaseAvailability(string connstring, CancellationToken cancellationToken, int executionLimit = 3000)
        {
            int result;

            Stopwatch watch = new();
            watch.Start();
            try
            {
                using SqlConnection conn = new(connstring);

                //open connection
                conn.Open();

                List<string> queryParts = new();

                queryParts.Add(Queries.AvailableDate1);
                queryParts.Add(Queries.AvailableDate2MinimumWarehousesBasic);
                queryParts.Add(Queries.AvailableDate3);
                queryParts.Add(Queries.AvailableDate4SourcesWithPrices);
                queryParts.Add(Queries.AvailableDate5);
                queryParts.Add(Queries.AvailableDate6IntervalsBasic);
                queryParts.Add(Queries.AvailableDate7);
                queryParts.Add(Queries.AvailableDate8DeliveryPowerBasic);
                queryParts.Add(Queries.AvailableDate9);

                string query = String.Join("", queryParts);

                var DateMove = DateTime.Now.AddMonths(24000);
                var TimeNow = new DateTime(2001, 1, 1, DateMove.Hour, DateMove.Minute, DateMove.Second);
                var EmptyDate = new DateTime(2001, 1, 1, 0, 0, 0);
                var MaxDate = new DateTime(5999, 11, 11, 0, 0, 0);

                SqlCommand cmd = new(query, conn);

                //FillGoodsTable(data, conn);

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

                cmd.CommandTimeout = 3;


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
                    ErrorDescription = "Availability false because of" + ex.Message,
                    Status = LogStatus.Error,
                    DatabaseConnection = LoadBalancing.RemoveCredentialsFromConnectionString(connstring)
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
                    DatabaseConnection = LoadBalancing.RemoveCredentialsFromConnectionString(connstring)
                };

                var logstringElement = JsonSerializer.Serialize(logElement);

                _logger.LogInformation(logstringElement);
            }


            return result >= 0;
        }

        public async Task<bool> CheckAggregationsAvailability(string connstring, CancellationToken cancellationToken, int executionLimit = 4500)
        {
            int result = -1;

            Stopwatch watch = new();
            watch.Start();
            try
            {
                using SqlConnection conn = new(connstring);

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
                    DatabaseConnection = LoadBalancing.RemoveCredentialsFromConnectionString(connstring)
                };

                var logstringElement = JsonSerializer.Serialize(logElement);

                _logger.LogInformation(logstringElement);

            }
            watch.Stop();

            //if (watch.ElapsedMilliseconds > executionLimit)
            //{
            //    result = -1;
            //}


            return result >= 0;
        }

    }
}

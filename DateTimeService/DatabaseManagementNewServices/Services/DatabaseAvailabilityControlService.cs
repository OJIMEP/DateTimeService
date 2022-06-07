using DateTimeService.Data;
using DateTimeService.DatabaseManagementNewServices.Interfaces;
using DateTimeService.DatabaseManagementUtils;
using DateTimeService.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DateTimeService.DatabaseManagementNewServices.Services
{
    public class DatabaseAvailabilityControlService : IDatabaseAvailabilityControl
    {
        private readonly IDatabaseCheck _databaseCheckService;
        private readonly IReadableDatabase _readableDatabaseService;
        private readonly ILogger<DatabaseAvailabilityControlService> _logger;
        private readonly IConfiguration _configuration;

        private readonly List<ClearCacheCriteria> clearCacheCriterias;
        private readonly string analyzeInterval = "now-1m";
        private readonly int errorsCountToSendClearCache;

        public DatabaseAvailabilityControlService(IDatabaseCheck databaseCheckService,
                                                  IReadableDatabase readableDatabaseService,
                                                  ILogger<DatabaseAvailabilityControlService> logger,
                                                  IConfiguration configuration)
        {
            _databaseCheckService = databaseCheckService;
            _readableDatabaseService = readableDatabaseService;
            _logger = logger;
            _configuration = configuration;

            clearCacheCriterias = _configuration.GetSection("ClearCacheCriterias").Get<List<ClearCacheCriteria>>();
            errorsCountToSendClearCache = _configuration.GetValue<int>("ErrorsCountToSendClearCache");
            if (errorsCountToSendClearCache == 0)
            {
                errorsCountToSendClearCache = 1;
            }
        }

        public async Task CheckAndUpdateDatabasesStatus(CancellationToken cancellationToken)
        {
            
            var dbList = _readableDatabaseService.GetAllDatabases();

            foreach (var databaseInfo in dbList)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (databaseInfo.AvailableToUse)
                {
                    string connection = databaseInfo.Connection;
                    string connectionWithoutCreds = databaseInfo.ConnectionWithoutCredentials;
                    int customAggsFailCount = databaseInfo.CustomAggsFailCount;
                    int timeCriteriaFailCount = databaseInfo.TimeCriteriaFailCount;

                    await CheckAndUpdateAggregations(connection, customAggsFailCount, cancellationToken);
                    await CheckAndUpdatePerfomance(connection, connectionWithoutCreds, timeCriteriaFailCount, cancellationToken);
                }
                else
                {
                    var availabilityResult = await _databaseCheckService.CheckAvailabilityAsync(databaseInfo.Connection, cancellationToken, 5000);
                    if (availabilityResult)
                    {
                        _readableDatabaseService.UpdateDatabaseLastChecksTime(databaseInfo.Connection, false, true, false, false);
                        _readableDatabaseService.EnableDatabase(databaseInfo.Connection);
                    }
                }
            }
        }

        private async Task CheckAndUpdatePerfomance(string connection, string connectionWithoutCreds, int timeCriteriaFailCount, CancellationToken cancellationToken)
        {
            var stats = await _databaseCheckService.GetElasticLogsInformationAsync(connection, cancellationToken);
            if (stats != null)
            {
                var dbAction = AnalyzeElasticResponse(stats);

                switch (dbAction)
                {
                    case DatabaseActions.Error:
                        break;
                    case DatabaseActions.None:
                        break;
                    case DatabaseActions.SendClearCache:


                        if (timeCriteriaFailCount > errorsCountToSendClearCache)
                        {
                            await SendClearCacheScript(connection, connectionWithoutCreds, cancellationToken);
                            _readableDatabaseService.UpdateDatabaseLastChecksTime(connection, true, false, false, false);
                        }
                        else
                        {
                            _readableDatabaseService.UpdateDatabasePerfomanceFailCount(connection, timeCriteriaFailCount, timeCriteriaFailCount + 1);
                        }

                        break;
                    case DatabaseActions.DisableZeroExecutionTime:
                        //TODO log it
                        _readableDatabaseService.DisableDatabase(connection);
                        break;
                    case DatabaseActions.DisableBigExecutionTime:
                        //TODO log it
                        _readableDatabaseService.DisableDatabase(connection);
                        break;
                    case DatabaseActions.DisableBigLoadBalanceTime:
                        //TODO log it
                        _readableDatabaseService.DisableDatabase(connection);
                        break;
                    default:
                        break;
                }

                _readableDatabaseService.UpdateDatabaseLastChecksTime(connection, false, false, false, true);
            }
            else
            {
                //todo log it
            }
        }

        private async Task CheckAndUpdateAggregations(string connection, int customAggsFailCount, CancellationToken cancellationToken)
        {
            var aggsResult = await _databaseCheckService.CheckAggregationsAsync(connection, cancellationToken);

            if (!aggsResult)
            {

                if (customAggsFailCount > 6)//TODO make config
                {
                    _readableDatabaseService.UpdateDatabaseAggregationsFailCount(connection, customAggsFailCount, 0);
                    _readableDatabaseService.DisableDatabaseAggs(connection);
                }
                else
                {
                    _readableDatabaseService.UpdateDatabaseAggregationsFailCount(connection, customAggsFailCount, customAggsFailCount + 1);
                }
            }
            else
            {
                _readableDatabaseService.UpdateDatabaseAggregationsFailCount(connection, customAggsFailCount, 0);
                _readableDatabaseService.EnableDatabaseAggs(connection);
            }

            _readableDatabaseService.UpdateDatabaseLastChecksTime(connection, false, false, true, false);
        }

        private async Task<bool> SendClearCacheScript(string databaseConnectionString, string databaseConnectionStringWithoutCredentials, CancellationToken cancellationToken)
        {
            bool result = false;

            try
            {
                using SqlConnection conn = new(databaseConnectionString);

                conn.Open();

                var clearCacheScript = Queries.ClearCacheScriptDefault;

                var clearCacheScriptFromConfig = _configuration.GetValue<string>("ClearCacheScript");

                if (!string.IsNullOrEmpty(clearCacheScriptFromConfig))
                {
                    clearCacheScript = clearCacheScriptFromConfig;
                }

                SqlCommand cmd = new(clearCacheScript, conn);

                cmd.CommandTimeout = 1;

                var clearCacheResult = await cmd.ExecuteNonQueryAsync(cancellationToken);
                conn.Close();

                //database.LastFreeProcCacheCommand = DateTimeOffset.Now;

                var logElement = new ElasticLogElement
                {
                    LoadBalancingExecution = 0,
                    ErrorDescription = "Send dbcc freeproccache",
                    Status = "Ok",
                    DatabaseConnection = databaseConnectionStringWithoutCredentials
                };

                var logstringElement = JsonSerializer.Serialize(logElement);

                _logger.LogInformation(logstringElement);

                result = true;
            }
            catch (Exception ex)
            {
                var logElement = new ElasticLogElement
                {
                    LoadBalancingExecution = 0,
                    ErrorDescription = ex.Message,
                    Status = "Error",
                    DatabaseConnection = databaseConnectionStringWithoutCredentials
                };

                var logstringElement = JsonSerializer.Serialize(logElement);

                _logger.LogError(logstringElement);

                result = false;
            }

            return result;
        }

        private DatabaseActions AnalyzeElasticResponse(ElasticDatabaseStats elasticStats)
        {
            var minutesInBucket = int.Parse(analyzeInterval.Replace("now-", "").Replace("m", ""));
            var recordsByMinute = elasticStats.RecordCount / minutesInBucket;
            var criteria = clearCacheCriterias.FirstOrDefault(s => recordsByMinute >= s.RecordCountBegin && recordsByMinute <= s.RecordCountEnd && s.CriteriaType == "RecordCount");
            var criteriaMaxTime = clearCacheCriterias.FirstOrDefault(s => s.CriteriaType == "MaximumResponseTime");
            var percentile95rate = elasticStats.Percentile95Time;

            if (criteria == default || percentile95rate == default)
            {
                return DatabaseActions.Error; //TODO log error
            }

            if (percentile95rate > criteria.Percentile_95)
            {
                return DatabaseActions.SendClearCache;
            }

            if (recordsByMinute >= 100 && elasticStats.AverageTime == 0)
            {
                return DatabaseActions.DisableZeroExecutionTime;
            }

            if (recordsByMinute >= 100 && elasticStats.LoadBalanceTime > criteriaMaxTime.LoadBalance)
            {
                return DatabaseActions.DisableBigLoadBalanceTime;
            }

            if (recordsByMinute >= 100 && elasticStats.AverageTime > criteriaMaxTime.Percentile_95)
            {
                return DatabaseActions.DisableBigExecutionTime;
            }           

            return DatabaseActions.None;
        }
    }
}

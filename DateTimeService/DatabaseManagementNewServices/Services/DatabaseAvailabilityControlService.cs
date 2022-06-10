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
        private readonly int delayBetweenClearCache;

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

            delayBetweenClearCache = _configuration.GetValue<int>("DelayBetweenClearCache");
            if (delayBetweenClearCache == 0)
            {
                delayBetweenClearCache = 180;
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
                    int customAggsFailCount = databaseInfo.CustomAggsFailCount;
                    int timeCriteriaFailCount = databaseInfo.TimeCriteriaFailCount;

                    if (databaseInfo.Type == "replica_tables")
                    {
                        await CheckAndUpdateAggregations(connection, customAggsFailCount, cancellationToken);
                    }
                    
                    await CheckAndUpdatePerfomance(connection, databaseInfo.LastFreeProcCacheCommand, timeCriteriaFailCount, databaseInfo.Type != "main", cancellationToken);
                }
                else
                {
                    if (databaseInfo.LastCheckAvailability == default 
                        || DateTimeOffset.Now - databaseInfo.LastCheckAvailability > TimeSpan.FromSeconds(60))
                    {
                        var availabilityResult = await _databaseCheckService.CheckAvailabilityAsync(databaseInfo.Connection, cancellationToken, 5000);
                        if (availabilityResult)
                        {
                            _readableDatabaseService.EnableDatabase(databaseInfo.Connection);
                        }
                        _readableDatabaseService.UpdateDatabaseLastAvailabilityCheckTime(databaseInfo.Connection);
                    }
                }
            }
        }

        private async Task CheckAndUpdatePerfomance(string connection, DateTimeOffset lastFreeProcCacheCommand, int timeCriteriaFailCount, bool clearCacheAllowed, CancellationToken cancellationToken)
        {
            if (!_configuration.GetValue<bool>("UseLoadBalance2"))
            {
                return;
            }

            var stats = await _databaseCheckService.GetElasticLogsInformationAsync(LoadBalancing.RemoveCredentialsFromConnectionString(connection), cancellationToken);
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
                        if (clearCacheAllowed)
                        {
                            await ProcessSendClearCacheAction(connection, lastFreeProcCacheCommand, timeCriteriaFailCount, cancellationToken);
                        }                        
                        break;
                    case DatabaseActions.DisableZeroExecutionTime:
                        _readableDatabaseService.DisableDatabase(connection, "zero execution time");
                        break;
                    case DatabaseActions.DisableBigExecutionTime:
                        _readableDatabaseService.DisableDatabase(connection, "big execution time");
                        break;
                    case DatabaseActions.DisableBigLoadBalanceTime:
                        _readableDatabaseService.DisableDatabase(connection, "big load balance time");
                        break;
                    default:
                        break;
                }
                _readableDatabaseService.UpdateDatabaseLastPerfomanceCheckTime(connection);
            }
            else
            {
                //todo log it
            }
        }

        private async Task ProcessSendClearCacheAction(string connection, DateTimeOffset lastFreeProcCacheCommand, int timeCriteriaFailCount, CancellationToken cancellationToken)
        {
            if (lastFreeProcCacheCommand == default
                                        || DateTimeOffset.Now - lastFreeProcCacheCommand > TimeSpan.FromSeconds(delayBetweenClearCache))
            {
                if (timeCriteriaFailCount > errorsCountToSendClearCache)
                {
                    await SendClearCacheScript(connection, cancellationToken);
                    _readableDatabaseService.UpdateDatabasePerfomanceFailCount(connection, timeCriteriaFailCount, 0);
                    _readableDatabaseService.UpdateDatabaseLastClearCacheTime(connection);
                }
                else
                {
                    _readableDatabaseService.UpdateDatabasePerfomanceFailCount(connection, timeCriteriaFailCount, timeCriteriaFailCount + 1);
                }
            }
            else
            {
                _readableDatabaseService.UpdateDatabasePerfomanceFailCount(connection, timeCriteriaFailCount, 0);
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

            _readableDatabaseService.UpdateDatabaseLastAggregationCheckTime(connection);
        }

        private async Task<bool> SendClearCacheScript(string databaseConnectionString, CancellationToken cancellationToken)
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
                    DatabaseConnection = LoadBalancing.RemoveCredentialsFromConnectionString(databaseConnectionString)
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
                    DatabaseConnection = LoadBalancing.RemoveCredentialsFromConnectionString(databaseConnectionString)
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

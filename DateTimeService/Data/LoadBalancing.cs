using DateTimeService.Controllers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using DateTimeService.DatabaseManagementNewServices.Interfaces;
using DateTimeService.Models;
using System.Linq;
using System.Threading;
using DateTimeService.Logging;

namespace DateTimeService.Data
{
    public interface ILoadBalancing
    {
        Task<DbConnection> GetDatabaseConnectionAsync(CancellationToken token = default);
    }

    public class LoadBalancing : ILoadBalancing
    {
        private readonly ILogger<DateTimeController> _logger;
        private readonly IReadableDatabase _databaseService;

        public LoadBalancing(ILogger<DateTimeController> logger, IReadableDatabase databaseService)
        {
            _logger = logger;
            _databaseService = databaseService;
        }

        public async Task<DbConnection> GetDatabaseConnectionAsync(CancellationToken token = default)
        {
            var result = new DbConnection();

            var watch = Stopwatch.StartNew();

            var connectionParameters = _databaseService.GetAllDatabases();

            var timeMS = DateTime.Now.Millisecond % 100;

            List<string> failedConnections = new();

            bool firstAvailable = false;

            var resultString = "";

            SqlConnection conn = null;

            while (true)
            {
                int percentCounter = 0;
                foreach (var connParameter in connectionParameters)
                {

                    if (firstAvailable && failedConnections.Contains(connParameter.Connection))
                        continue;

                    if (!connParameter.AvailableToUse)
                        continue;

                    percentCounter += connParameter.Priority;
                    if ((timeMS <= percentCounter && connParameter.Priority != 0) || firstAvailable)
                        try
                        {
                            conn = await GetConnectionByDatabaseInfo(connParameter, token);

                            result.Connection = conn;
                            resultString = connParameter.Connection;
                            result.DatabaseType = connParameter.DatabaseType;
                            result.UseAggregations = connParameter.CustomAggregationsAvailable;
                            result.ConnectionWithoutCredentials = connParameter.ConnectionWithoutCredentials;
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (watch.IsRunning)
                            {
                                watch.Stop();
                            }

                            var logElement = new ElasticLogElement
                            {
                                ErrorDescription = ex.Message,
                                Status = LogStatus.Error,
                                DatabaseConnection = connParameter.ConnectionWithoutCredentials
                            };

                            var logstringElement = JsonSerializer.Serialize(logElement);

                            _logger.LogMessageGen(logstringElement);

                            if (conn != null && conn.State != System.Data.ConnectionState.Closed)
                            {
                                conn.Close();
                            }

                            failedConnections.Add(connParameter.Connection);
                        }
                }
                if (resultString.Length > 0 || firstAvailable)
                    break;
                else
                    firstAvailable = true;
            }

            watch.Stop();
            result.ConnectTimeInMilliseconds = watch.ElapsedMilliseconds;

            return result;
        }

        private static async Task<SqlConnection> GetConnectionByDatabaseInfo(DatabaseInfo databaseInfo, CancellationToken token = default)
        {
            var queryStringCheck = databaseInfo.DatabaseType switch
            {
                DatabaseType.Main => Queries.DatabaseBalancingMain,
                DatabaseType.ReplicaFull => Queries.DatabaseBalancingReplicaFull,
                DatabaseType.ReplicaTables => Queries.DatabaseBalancingReplicaTables,
                _ => ""
            };

            //sql connection object
            SqlConnection connection = new(databaseInfo.Connection);
            await connection.OpenAsync(token);

            SqlCommand cmd = new(queryStringCheck, connection)
            {
                CommandTimeout = 1
            };

            SqlDataReader dr = await cmd.ExecuteReaderAsync(token);

            _ = dr.CloseAsync();

            return connection;
        }

        public static string RemoveCredentialsFromConnectionString(string connectionString)
        {
            return string.Join(";",
                connectionString.Split(";")
                    .Where(item => !item.Contains("Uid") && !item.Contains("User") && !item.Contains("Pwd") && !item.Contains("Password") && item.Length > 0));
        }

    }

    public class DatabaseConnectionParameter
    {
        public string Connection { get; set; }
        public int Priority { get; set; }
        public string Type { get; set; } //main, replica_full, replica_tables 

    }

    public class DbConnection
    {
        public SqlConnection Connection { get; set; }
        public DatabaseType DatabaseType { get; set; }
        public bool UseAggregations { get; set; }
        public string ConnectionWithoutCredentials { get; set; } = "";
        public long ConnectTimeInMilliseconds { get; set; }
    }
}

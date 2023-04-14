﻿using DateTimeService.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using DateTimeService.DatabaseManagementNewServices.Interfaces;

namespace DateTimeService.Data
{
    public interface ILoadBalancing
    {
        Task<DbConnection> GetDatabaseConnectionAsync();
    }

    public class LoadBalancing : ILoadBalancing
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DateTimeController> _logger;
        private readonly IReadableDatabase _databaseService;

        public LoadBalancing(IConfiguration configuration, ILogger<DateTimeController> logger, IReadableDatabase databaseService)
        {
            _configuration = configuration;
            _logger = logger;
            _databaseService = databaseService;
        }

        public async Task<DbConnection> GetDatabaseConnectionAsync()
        {
            //string connString = _configuration.GetConnectionString("1CDataSqlConnection");

            var result = new DbConnection();

            var watch = Stopwatch.StartNew();

            var connectionParameters = DatabaseList.Databases;//_configuration.GetSection("OneSDatabases").Get<List<DatabaseConnectionParameter>>();

            connectionParameters = _databaseService.GetAllDatabases();

            var timeMS = DateTime.Now.Millisecond % 100;

            List<string> failedConnections = new();

            bool firstAvailable = false;

            var resultString = "";

            //SqlConnection resultConnection = null;
            SqlConnection conn = null;

            while (true)
            {
                int percentCounter = 0;
                foreach (var connParametr in connectionParameters)
                {

                    if (firstAvailable && failedConnections.Contains(connParametr.Connection))
                        continue;

                    if (!connParametr.AvailableToUse)
                        continue;

                    percentCounter += connParametr.Priority;
                    if ((timeMS <= percentCounter && connParametr.Priority != 0) || firstAvailable)
                        try
                        {
                            var queryStringCheck = "";
                            if (connParametr.Type == "main")
                                queryStringCheck = Queries.DatebaseBalancingMain;

                            if (connParametr.Type == "replica_full")
                                queryStringCheck = Queries.DatebaseBalancingReplicaFull;

                            if (connParametr.Type == "replica_tables")
                                queryStringCheck = Queries.DatebaseBalancingReplicaTables;


                            //sql connection object
                            conn = new(connParametr.Connection);


                            conn.Open();

                            SqlCommand cmd = new(queryStringCheck, conn);

                            cmd.CommandTimeout = 1;

                            SqlDataReader dr = await cmd.ExecuteReaderAsync();

                            dr.Close();
                            
                            //close connection
                            //conn.Close();
                            result.Connection = conn;
                            resultString = connParametr.Connection;
                            result.DatabaseType = connParametr.Type;
                            result.UseAggregations = connParametr.CustomAggregationsAvailable;
                            result.ConnectionWithoutCredentials = connParametr.ConnectionWithoutCredentials;
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
                                Status = "Error",
                                DatabaseConnection = connParametr.ConnectionWithoutCredentials
                            };

                            var logstringElement = JsonSerializer.Serialize(logElement);

                            _logger.LogInformation(logstringElement);

                            if (conn != null && conn.State != System.Data.ConnectionState.Closed)
                            {
                                conn.Close();
                            }

                            failedConnections.Add(connParametr.Connection);
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

        public static string RemoveCredentialsFromConnectionString(string connectionString)
        {
            var connStringParts = connectionString.Split(";");

            var resultString = "";

            foreach (var item in connStringParts)
            {
                if (!item.Contains("Uid") && !item.Contains("User") && !item.Contains("Pwd") && !item.Contains("Password") && item.Length > 0)
                    resultString += (item + ";");
            }

            return resultString;
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
        public string DatabaseType { get; set; }
        public bool UseAggregations { get; set; }
        public string ConnectionWithoutCredentials { get; set; } = "";
        public long ConnectTimeInMilliseconds { get; set; }
    }
}

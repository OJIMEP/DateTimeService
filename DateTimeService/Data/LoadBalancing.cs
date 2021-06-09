using DateTimeService.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DateTimeService.Data
{
    public interface ILoadBalancing
    {
        Task<SqlConnection> GetDatabaseConnectionAsync();
    }

    public class LoadBalancing : ILoadBalancing
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DateTimeController> _logger;

        public LoadBalancing(IConfiguration configuration, ILogger<DateTimeController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<SqlConnection> GetDatabaseConnectionAsync()
        {
            //string connString = _configuration.GetConnectionString("1CDataSqlConnection");
            
            var connectionParameters = _configuration.GetSection("OneSDatabases").Get<List<DatabaseConnectionParameter>>();

            var timeMS = DateTime.Now.Millisecond % 100;

            List<string> failedConnections = new();

            bool firstAvailable = false;

            var result = "";

            SqlConnection resultConnection = null;
            SqlConnection conn = null;

            while (true)
            {
                int percentCounter = 0;
                foreach (var connParametr in connectionParameters)
                {

                    if (firstAvailable && failedConnections.Contains(connParametr.Connection))
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
                            resultConnection = conn;
                            result = connParametr.Connection;
                            break;
                        }
                        catch (Exception ex)
                        {
                            var logElement = new ElasticLogElement
                            {
                                TimeSQLExecution = 0,
                                ErrorDescription = ex.Message,
                                Status = "Error",
                                DatabaseConnection = LoadBalancing.RemoveCredentialsFromConnectionString(connParametr.Connection)
                            };

                            var logstringElement = JsonSerializer.Serialize(logElement);

                            _logger.LogInformation(logstringElement);
                            
                            if (conn != null && conn.State != System.Data.ConnectionState.Closed )
                            {
                                conn.Close();
                            }

                            failedConnections.Add(connParametr.Connection);
                        }
                }
                if (result.Length > 0 || firstAvailable)
                    break;
                else
                    firstAvailable = true;
            }

            return resultConnection;
        }

        public static string RemoveCredentialsFromConnectionString(string connectionString)
        {
            var connStringParts = connectionString.Split(";");

            var resultString = "";

            foreach (var item in connStringParts)
            {
                if (!item.Contains("Uid") && !item.Contains("User") && !item.Contains("Pwd") && !item.Contains("Password") && item.Length>0)
                    resultString += (item+";");
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

  

}

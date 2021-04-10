using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DateTimeService.Data
{
    public interface ILoadBalancing
    {
        //Task<AuthenticateResponse> AuthenticateAsync(LoginModel model, string ipAddress);
        //Task<AuthenticateResponse> RefreshTokenAsync(string token, string ipAddress);
        //bool RevokeToken(string token, string ipAddress);
        //IEnumerable<DateTimeServiceUser> GetAll();
        //DateTimeServiceUser GetById(int id);
        Task<string> GetDatabaseConnectionAsync();
    }

    public class LoadBalancing : ILoadBalancing
    {
        private readonly IConfiguration _configuration;

        public LoadBalancing(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GetDatabaseConnectionAsync()
        {
            string connString = _configuration.GetConnectionString("1CDataSqlConnection");
            
            var connectionParameters = _configuration.GetSection("OneSDatabases").Get<List<DatabaseConnectionParameter>>();

            var timeMS = DateTime.Now.Millisecond;
            
            bool firstAvailable = false;

            var result = "";

            while (true)
            {
                int percentCounter = 0;
                foreach (var connParametr in connectionParameters)
                {
                    percentCounter += connParametr.Priority;
                    if (timeMS <= percentCounter*10 || firstAvailable)
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
                            using SqlConnection conn = new(connParametr.Connection);

                            conn.Open();

                            SqlCommand cmd = new(queryStringCheck, conn);

                            SqlDataReader dr = await cmd.ExecuteReaderAsync();
                            
                            dr.Close();

                            //close connection
                            conn.Close();

                            result = connParametr.Connection;
                            break;
                        }
                        catch (Exception ex)
                        {
                            connString = ex.Message;
                        }
                }
                if (result.Length > 0)
                    break;
                else
                    firstAvailable = true;
            }

            return result;
        }


    }

    public class DatabaseConnectionParameter
    {
        public string Connection { get; set; }
        public int Priority { get; set; }
        public string Type { get; set; } //main, replica_full, replica_tables 

    }

}

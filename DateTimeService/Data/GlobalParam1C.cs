﻿using DateTimeService.Controllers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text.Json;

namespace DateTimeService.Data
{
    public class GlobalParam1C
    {
        public string Name { get; set; }
        public double ValueDouble { get; set; }
        public double DefaultDouble { get; set; }
        public Boolean UseDefault { get; set; }


        public static Boolean FillValues(SqlConnection conn, List<GlobalParam1C> names, ILogger<DateTimeController> _logger)
        {

            bool querySuccessful = false;

            try
            {
                //sql connection object
                //using SqlConnection conn = new(connString);



                string queryParametrs = @"SELECT [_Fld22354]      
        ,[_Fld22355_TYPE]     
        ,[_Fld22355_N]     
        ,[_Fld22355_L]     
  FROM [dbo].[_InfoRg22353]
  where [_Fld22354] IN({0})";

                SqlCommand cmd = new(queryParametrs, conn);

                cmd.CommandTimeout = 1;

                var parameters = new string[names.Count];
                for (int i = 0; i < names.Count; i++)
                {
                    parameters[i] = string.Format("@Article{0}", i);
                    cmd.Parameters.AddWithValue(parameters[i], names[i].Name);
                }

                cmd.CommandText = string.Format(queryParametrs, string.Join(", ", parameters));

                //conn.Open();

                //execute the SQLCommand
                SqlDataReader drParametrs = cmd.ExecuteReader();

                //check if there are records
                if (drParametrs.HasRows)
                {
                    while (drParametrs.Read())
                    {
                        if (((byte[]) drParametrs.GetValue(1))[0] == 2) //boalean
                        {
                            names.First(x => x.Name.Contains(drParametrs.GetString(0))).ValueDouble = (double)((byte[])drParametrs.GetValue(3))[0];
                        }
                        else
                        {
                            names.First(x => x.Name.Contains(drParametrs.GetString(0))).ValueDouble = (double)drParametrs.GetDecimal(2);
                        }
                        

                        querySuccessful = true;
                    }
                }

                foreach (var param in names)
                {
                    if (param.UseDefault && param.ValueDouble == 0)
                    {
                        param.ValueDouble = param.DefaultDouble;
                    }
                }

                //close data reader
                drParametrs.Close();

                //close connection
                //conn.Close();

            }
            catch (Exception ex)
            {
                var logElement = new ElasticLogElement
                {
                    TimeSQLExecution = 0,
                    ErrorDescription = ex.Message,
                    Status = "Error",
                    DatabaseConnection = LoadBalancing.RemoveCredentialsFromConnectionString(conn.ConnectionString)
                };

                var logstringElement = JsonSerializer.Serialize(logElement);

                _logger.LogInformation(logstringElement);

                querySuccessful = false;
            }

            return querySuccessful;
        }
    }


}

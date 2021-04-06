using DateTimeService.Controllers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DateTimeService.Data
{
    public class GeoAdress
    {

        public static Boolean AdressExists(string connString, string _addressId, ILogger<DateTimeController> _logger)
        {

            bool result = false;

            try
            {
                //sql connection object
                using SqlConnection conn = new(connString);



                string queryParametrs = @"Select Top 1 --по адресу находим геозону
	ГеоАдрес._Fld2785RRef 
	From dbo._Reference112 ГеоАдрес With (NOLOCK)
	Where ГеоАдрес._Fld25155 = @P4 
    AND ГеоАдрес._Marked = 0x00
    AND ГеоАдрес._Fld2785RRef <> 0x00000000000000000000000000000000";

                SqlCommand cmd = new(queryParametrs, conn);

                cmd.Parameters.AddWithValue("@P4", _addressId);


                cmd.CommandText = queryParametrs;

                conn.Open();

                //execute the SQLCommand
                SqlDataReader drParametrs = cmd.ExecuteReader();

                //check if there are records
                if (drParametrs.HasRows)
                {
                    result = true;
                }

                //close data reader
                drParametrs.Close();

                //close connection
                conn.Close();

            }
            catch (Exception ex)
            {
                var logElement = new ElasticLogElement
                {
                    TimeSQLExecution = 0,
                    ErrorDescription = ex.Message,
                    Status = "Error"
                };

                var logstringElement = JsonSerializer.Serialize(logElement);

                _logger.LogInformation(logstringElement);

                result = false;
            }

            return result;
        }
    }
}

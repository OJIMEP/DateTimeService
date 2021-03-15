using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DateTimeService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DateTimeController : ControllerBase
    {
        private readonly ILogger<DateTimeController> _logger;

        public DateTimeController(ILogger<DateTimeController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IEnumerable<ResponseElement> Post(IEnumerable<RequestData> nomenclatures)
        {

            string connString = @"Server=tcp:192.168.1.14;Database=triovist;Uid=sa;Pwd=examplePass; Trusted_Connection = False;";

            //string connString = @"Server=localhost;Database=DevBase_cut_v3;Uid=sa;Pwd=; Trusted_Connection = False;";

            string parametr2 = "";

            foreach (var item in nomenclatures)
            {
                parametr2 += item.Nomenclature_id+",";
            }

            parametr2 = parametr2.Remove(parametr2.Length - 1);

            var result = new List<ResponseElement>();

            _logger.LogInformation(HttpContext.Request.ToString());

            try
            {
                //sql connection object
                using SqlConnection conn = new(connString);

                //retrieve the SQL Server instance version
                //                   string query = @"SELECT
                //T4._Fld3480 AS nomenclature_id,
                //CAST(SUM((T1.Fld21411Balance_ - T1.Fld21412Balance_)) AS NUMERIC(34, 3)) AS max_available_count 
                //FROM (SELECT
                //T2._Fld21408RRef AS Fld21408RRef,
                //CAST(SUM(T2._Fld21412) AS NUMERIC(27, 3)) AS Fld21412Balance_,
                //CAST(SUM(T2._Fld21411) AS NUMERIC(27, 3)) AS Fld21411Balance_
                //FROM dbo._AccumRgT21444 T2 WITH(NOLOCK)
                //LEFT OUTER JOIN dbo._Reference149 T3 WITH(NOLOCK)
                //ON T2._Fld21408RRef = T3._IDRRef
                //WHERE T2._Period = '5999-11-01 00:00:00' AND (((T2._Fld21424 = '2001-01-01 00:00:00') OR (T2._Fld21424 >= @P1)) AND (T3._Fld3480 IN (@P2)) AND (T3._Fld3514RRef = 0x84A6131B6DC5555A4627E85757507687)) AND (T2._Fld21412 <> 0 OR T2._Fld21411 <> 0) AND (T2._Fld21412 <> 0 OR T2._Fld21411 <> 0)
                //GROUP BY T2._Fld21408RRef
                //HAVING (CAST(SUM(T2._Fld21412) AS NUMERIC(27, 3))) <> 0.0 OR (CAST(SUM(T2._Fld21411) AS NUMERIC(27, 3))) <> 0.0) T1
                //LEFT OUTER JOIN dbo._Reference149 T4 WITH(NOLOCK)
                //ON T1.Fld21408RRef = T4._IDRRef
                //WHERE ((T1.Fld21411Balance_ - T1.Fld21412Balance_) > 0)
                //GROUP BY T1.Fld21408RRef,
                //T4._Fld3480";

                string query = @"SELECT
                                        T4._Fld3480 AS nomenclature_id,
                                        CAST(SUM((T1.Fld21411Balance_ - T1.Fld21412Balance_)) AS NUMERIC(34, 3)) AS max_available_count 
                                        FROM (SELECT
                                        T2._Fld21408RRef AS Fld21408RRef,
                                        CAST(SUM(T2._Fld21412) AS NUMERIC(27, 3)) AS Fld21412Balance_,
                                        CAST(SUM(T2._Fld21411) AS NUMERIC(27, 3)) AS Fld21411Balance_
                                        FROM dbo._AccumRgT21444 T2 WITH(NOLOCK)
                                        LEFT OUTER JOIN dbo._Reference149 T3 WITH(NOLOCK)
                                        ON T2._Fld21408RRef = T3._IDRRef
                                        WHERE T2._Period = '5999-11-01 00:00:00' AND (((T2._Fld21424 = '2001-01-01 00:00:00') OR (T2._Fld21424 >= @P1)) AND (T3._Fld3480 IN ({0})) AND (T3._Fld3514RRef = 0x84A6131B6DC5555A4627E85757507687)) AND (T2._Fld21412 <> 0 OR T2._Fld21411 <> 0) AND (T2._Fld21412 <> 0 OR T2._Fld21411 <> 0)
                                        GROUP BY T2._Fld21408RRef
                                        HAVING (CAST(SUM(T2._Fld21412) AS NUMERIC(27, 3))) <> 0.0 OR (CAST(SUM(T2._Fld21411) AS NUMERIC(27, 3))) <> 0.0) T1
                                        LEFT OUTER JOIN dbo._Reference149 T4 WITH(NOLOCK)
                                        ON T1.Fld21408RRef = T4._IDRRef
                                        WHERE ((T1.Fld21411Balance_ - T1.Fld21412Balance_) > 0)
                                        GROUP BY T1.Fld21408RRef,
                                        T4._Fld3480";


                var DateMove = DateTime.Now.AddMonths(24000);

                //define the SqlCommand object
                SqlCommand cmd = new(query, conn);

                cmd.Parameters.Add("@P1", SqlDbType.DateTime);
                cmd.Parameters["@P1"].Value = DateMove;


                var parameters = new string[nomenclatures.Count()];
                for (int i = 0; i < nomenclatures.Count(); i++)
                {
                    parameters[i] = string.Format("@Article{0}", i);
                    cmd.Parameters.AddWithValue(parameters[i], nomenclatures.ToList()[i].Nomenclature_id);
                }

                cmd.CommandText = string.Format(query, string.Join(", ", parameters));



                //open connection
                conn.Open();

                //execute the SQLCommand
                SqlDataReader dr = cmd.ExecuteReader();

                //Console.WriteLine(Environment.NewLine + "Retrieving data from database..." + Environment.NewLine);
                //Console.WriteLine("Retrieved records:");

                //check if there are records
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {

                        var resultItem = new ResponseElement();
                        resultItem.Nomenclature_id = dr.GetString(0);
                        resultItem.Max_available_count = dr.GetDecimal(1);

                        result.Add(resultItem);

                        //empID = dr.GetInt32(0);
                        //Console.WriteLine("{0},{1}", resultItem.Nomenclature_id, resultItem.Max_available_count.ToString());
                    }
                }
                else
                {
                    //Console.WriteLine("No data found.");
                }

                //close data reader
                dr.Close();

                //close connection
                conn.Close();
            }
            catch (Exception ex)
            {
                //display error message
                Console.WriteLine("Exception: " + ex.Message);
            }

            return result.ToArray();
        }
    }
}

using DateTimeService.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace DateTimeService.Data
{
    public interface IGeoZones
    {
        Task<string> GetGeoZoneID(AdressCoords coords);
        Task<AdressCoords> GetAddressCoordinates(string address_id);
        Boolean AdressExists(SqlConnection conn, string _addressId);
    }

    public class GeoZones : IGeoZones
    {

        private readonly ILogger<DateTimeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        public GeoZones(ILogger<DateTimeController> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }


        public Boolean AdressExists(SqlConnection conn, string _addressId)
        {

            bool result = false;

            try
            {
                //sql connection object
                //using SqlConnection conn = new(connString);



                string queryParametrs = @"Select Top 1 --по адресу находим геозону
	ГеоАдрес._Fld2785RRef 
	From dbo._Reference112 ГеоАдрес With (NOLOCK)
	Where ГеоАдрес._Fld25155 = @P4 
    AND ГеоАдрес._Marked = 0x00
    AND ГеоАдрес._Fld2785RRef <> 0x00000000000000000000000000000000";

                SqlCommand cmd = new(queryParametrs, conn);

                cmd.Parameters.AddWithValue("@P4", _addressId);

                cmd.CommandTimeout = 1;

                cmd.CommandText = queryParametrs;

                //conn.Open();

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

                result = false;
            }

            return result;
        }

        public async Task<AdressCoords> GetAddressCoordinates(string address_id)
        {

            AdressCoords result = null;
            

            var client = _httpClientFactory.CreateClient();

            client.Timeout = new TimeSpan(0, 0, 8);
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");
            //client.DefaultRequestHeaders.Add("Content-Type", "application/vnd.api+json");

            string connString = _configuration.GetConnectionString("api21vekby_location");


            if (!int.TryParse(_configuration["LocationMicroserviceVersion"], out int locationVersion))
                locationVersion = 1;

            var uri = new Uri(connString + address_id);
            HttpRequestMessage request = new(HttpMethod.Get, uri)
            {
                Content = new StringContent("{}",
                                                Encoding.UTF8,
                                                "application/vnd.api+json")//CONTENT-TYPE header
            };


            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            try
            {
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (locationVersion == 1)
                    {
                        var locationsResponse = JsonSerializer.Deserialize<LocationsResponse>(responseString);

                        result = new(locationsResponse.Data.Attributes.X_coordinate, locationsResponse.Data.Attributes.Y_coordinate);

                    }
                    else if (locationVersion == 2)
                    {
                        IFormatProvider formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };

                        var locationsResponse = JsonSerializer.Deserialize<LocationsResponseV2>(responseString);

                        foreach (var item in locationsResponse.Data)
                        {

                            result = new(item.Attributes.X_coordinate, item.Attributes.Y_coordinate);

                            break;
                        }
                    }                    
                }              
            }
            catch(FormatException )
            {
                var logElement = new ElasticLogElement
                {
                    TimeSQLExecution = 0,
                    ErrorDescription = "Некорректные координаты адреса",
                    Status = "Error",
                    DatabaseConnection = connString
                };
                logElement.AdditionalData.Add("address_id", address_id);
                var logstringElement = JsonSerializer.Serialize(logElement);

                _logger.LogInformation(logstringElement);
                
            }
            catch (Exception ex)
            {
                var logElement = new ElasticLogElement
                {
                    TimeSQLExecution = 0,
                    ErrorDescription = ex.Message,
                    Status = "Error",
                    DatabaseConnection = connString
                };

                var logstringElement = JsonSerializer.Serialize(logElement);

                _logger.LogInformation(logstringElement);                
            }

            return result;
        }

        public async Task<string> GetGeoZoneID(AdressCoords coords)
        {
            string connString = _configuration.GetConnectionString("BTS_zones");
            string login = _configuration.GetValue<string>("BTS_login");
            string pass = _configuration.GetValue<string>("BTS_pass");

            var request = new HttpRequestMessage(HttpMethod.Post,
            connString);
            //request.Headers.Add("Content-Type", "text/xml");
            request.Headers.Add("User-Agent", "HttpClientFactory-Sample");
            IFormatProvider formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };
            string content = @"
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <m:getZoneByCoords xmlns:m=""http://ws.vrptwserver.beltranssat.by/"">
            <m:latitude xmlns:xs=""http://www.w3.org/2001/XMLSchema"" 
     xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">{0}</m:latitude>
            <m:longitude xmlns:xs=""http://www.w3.org/2001/XMLSchema"" 
     xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">{1}</m:longitude>
            <m:geomNeeded xmlns:xs=""http://www.w3.org/2001/XMLSchema"" 
     xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">false</m:geomNeeded>
        </m:getZoneByCoords>
    </soap:Body>
</soap:Envelope>";

            content = string.Format(content, coords.X_coordinates.ToString(formatter), coords.Y_coordinates.ToString(formatter));

            request.Content = new StringContent(content, Encoding.UTF8, "text/xml");

            var authenticationString = login + ":" + pass;
            var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));

            var client = new HttpClient();
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);


            var response = await client.SendAsync(request);
            string result = "";
            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var xml = new XmlSerializer(typeof(Envelope));
                var responseData = (Envelope)xml.Deserialize(responseStream);
                result = responseData.Items[0].getZoneByCoordsResponse.zone.id;
            }
            else
            {
                var logElement = new ElasticLogElement
                {
                    TimeSQLExecution = 0,
                    ErrorDescription = response.ToString(),
                    Status = "Error",
                    DatabaseConnection = connString
                };

                var logstringElement = JsonSerializer.Serialize(logElement);

                _logger.LogInformation(logstringElement);

            }
            return result;
        }

    }

    public class LocationsResponse
    {
        [JsonPropertyName("data")]
        public LocationsResponseElem Data { get; set; }
    }

    public class LocationsResponseV2
    {
        [JsonPropertyName("data")]
        public List<LocationsResponseElemV2> Data { get; set; }
    }

    public class LocationsResponseElem
    {
        [JsonPropertyName("attributes")]
        public LocationsResponseElemAttrib Attributes { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }

    public class LocationsResponseElemV2
    {
        [JsonPropertyName("attributes")]
        public LocationsResponseElemAttrib Attributes { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("id")]
        public decimal Id { get; set; }
    }

    public class LocationsResponseElemAttrib
    {
        [JsonPropertyName("x_coordinate")]
        public string X_coordinate { get; set; }
        [JsonPropertyName("y_coordinate")]
        public string Y_coordinate { get; set; }
    }
}

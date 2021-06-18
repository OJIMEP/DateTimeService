using DateTimeService.Controllers;
using DateTimeService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DateTimeService.Data
{
    public class DatabaseManagement
    {

        private readonly ILogger<DateTimeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public DatabaseManagement(ILogger<DateTimeController> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task DoSomething(CancellationToken cancellationToken)
        {

            _logger.LogInformation("database management executed");

            var _httpClient = new HttpClient();

            try
            {
                var response = await _httpClient.GetAsync("", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var RandomString = await response.Content.ReadAsStringAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task CheckDatabaseStatus(CancellationToken cancellationToken)
        {

            _logger.LogInformation("Database management service execution started");
            var elasticHost = _configuration["elasticsearch:host"];
            var elasticPort = _configuration["elasticsearch:port"];
            var elasticLogin = _configuration["elasticsearch:login"];
            var elasticPass = _configuration["elasticsearch:password"];
            var indexPath = _configuration["elasticsearch:indexName"];
            var authenticationString = elasticLogin + ":" + elasticPass; 
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));

            ElasticResponse elasticResponse = null;

            var httpClient = _httpClientFactory.CreateClient("elastic");

            UriBuilder elasticUri = new("https", elasticHost, Int32.Parse(elasticPort), indexPath + "/_search");

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var serverIP = ipHostInfo.AddressList.Where(s => s.AddressFamily == AddressFamily.InterNetwork).First().ToString();
            HttpRequestMessage requestMessage = new(HttpMethod.Get, elasticUri.Uri);
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

            var searchrequest = new ElasticRequest
            {
                Size = 0
            };

            FilterElement element = new();
            element.Range = new();
            element.Range.Add("@timestamp", new { gt = "now-2m" });

            searchrequest.Query.Bool.Filter.Add(element);

            element = new();
            element.Term = new();
            element.Term.Add("server_host.keyword", new { value = serverIP });
            searchrequest.Query.Bool.Filter.Add(element);


            AggregationClass rootAgg = new();
            rootAgg.Terms = new();
            rootAgg.Terms.Field = "DatabaseConnection.keyword";
            rootAgg.Terms.Size = 5;

            rootAgg.Aggregations = new();

            AggregationClass timePercentile = new();
            timePercentile.Percentiles = new();
            timePercentile.Percentiles.Field = "TimeFullExecution";
            timePercentile.Percentiles.Percents = new double[] { 95,99,99.5};
           
            rootAgg.Aggregations.Add("time_percentile", timePercentile);

            AggregationClass average = new();
            average.Avg = new();
            average.Avg.Field = "TimeFullExecution";
            rootAgg.Aggregations.Add("week_avg", average);

            searchrequest.Aggregations.Add("load_time_outlier", rootAgg);

            var content = JsonSerializer.Serialize(searchrequest);
            requestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");


            var result = "";

            try
            {
                var response = await httpClient.SendAsync(requestMessage, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsStringAsync(cancellationToken);
                }
                else
                {
                    var errodData = await response.Content.ReadAsStringAsync(cancellationToken);
                    var logElement = new ElasticLogElement
                    {
                        ErrorDescription = errodData,
                        Status = "Error",
                        DatabaseConnection = elasticUri.ToString()
                    };

                    logElement.AdditionalData.Add("requestContent", content);
                    var logstringElement = JsonSerializer.Serialize(logElement);

                    _logger.LogError(logstringElement);
                }
                    
               
            }
            catch (Exception ex)
            {
                var logElement = new ElasticLogElement
                {
                    TimeSQLExecution = 0,
                    ErrorDescription = ex.Message,
                    Status = "Error",
                    DatabaseConnection = elasticUri.ToString()
                };

                var logstringElement = JsonSerializer.Serialize(logElement);

                _logger.LogError(logstringElement);
            }


            try
            {
                if (!String.IsNullOrEmpty(result))
                {
                    elasticResponse = JsonSerializer.Deserialize<ElasticResponse>(result);
                }
            }
            catch (Exception ex)
            {
                var logElement = new ElasticLogElement
                {
                    TimeSQLExecution = 0,
                    ErrorDescription = ex.Message,
                    Status = "Error",
                    DatabaseConnection = elasticUri.ToString()
                };
                logElement.AdditionalData.Add("responseContent", result);
                var logstringElement = JsonSerializer.Serialize(logElement);

                _logger.LogError(logstringElement);
            }

            if (elasticResponse == null)
            {
                return;
            }

            foreach (var rootAggregation in elasticResponse.Aggregations)
            {
                if (rootAggregation.Key == "load_time_outlier")
                {
                    foreach (var bucket in rootAggregation.Value.Buckets)
                    {
                        
                        

                    }
                }
            }
        }
    }
}

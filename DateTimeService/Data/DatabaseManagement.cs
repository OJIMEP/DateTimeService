using DateTimeService.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DateTimeService.Data
{
    public class DatabaseManagement
    {

        private readonly ILogger<DateTimeController> _logger;
        private readonly IConfiguration _configuration;

        public DatabaseManagement(ILogger<DateTimeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
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
    }
}

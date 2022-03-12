using DateTimeService.Controllers;
using DateTimeService.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DateTimeService.DatabaseManagementUtils
{
    public class ReloadDatabasesFromFileService : HostedService
    {

        private readonly IConfiguration _configuration;
        private readonly ILogger<DateTimeController> _logger;

        public ReloadDatabasesFromFileService(ILogger<DateTimeController> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {

                var dbList = _configuration.GetSection("OneSDatabases").Get<List<DatabaseConnectionParameter>>();

                if (dbList == null)
                {
                    dbList = new();
                }
                await DatabaseList.CreateUpdateDatabases(dbList, _logger);

                await Task.Delay(TimeSpan.FromSeconds(11), cancellationToken);
            }
        }
    }
}

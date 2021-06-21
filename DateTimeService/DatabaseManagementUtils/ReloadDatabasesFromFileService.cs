using DateTimeService.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DateTimeService.DatabaseManagementUtils
{
    public class ReloadDatabasesFromFileService : HostedService
    {

        private readonly IConfiguration _configuration;

        public ReloadDatabasesFromFileService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {

                await DatabaseList.CreateUpdateDatabases(_configuration.GetSection("OneSDatabases").Get<List<DatabaseConnectionParameter>>());
                
                await Task.Delay(TimeSpan.FromSeconds(11), cancellationToken);
            }
        }
    }
}

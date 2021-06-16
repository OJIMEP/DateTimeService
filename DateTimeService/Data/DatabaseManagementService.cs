using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DateTimeService.Data
{
    public class DatabaseManagementService : HostedService
    {
        private readonly DatabaseManagement _databaseManagement;

        public DatabaseManagementService(DatabaseManagement randomStringProvider)
        {
            _databaseManagement = randomStringProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _databaseManagement.DoSomething(cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace DateTimeService.Data
{
    public class DatabaseManagementService : HostedService
    {
        private readonly DatabaseManagement _databaseManagement;

        public DatabaseManagementService(DatabaseManagement databaseManagement)
        {
            _databaseManagement = databaseManagement;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _databaseManagement.CheckDatabaseStatus(cancellationToken);


                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
        }
    }
}

using DateTimeService.Models;
using System.Threading;
using System.Threading.Tasks;

namespace DateTimeService.DatabaseManagementNewServices.Interfaces
{
    public interface IDatabaseCheck
    {
        public Task<ElasticDatabaseStats> GetElasticLogsInformationAsync(string databaseConnectionWithOutCredentials, CancellationToken cancellationToken);

        public Task<bool> CheckAvailabilityAsync(string databaseConnectionString, CancellationToken cancellationToken, long executionLimit = 5000);

        public Task<bool> CheckAggregationsAsync(string databaseConnectionString, CancellationToken cancellationToken);


    }
}

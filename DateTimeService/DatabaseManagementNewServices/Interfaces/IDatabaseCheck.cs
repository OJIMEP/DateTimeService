using DateTimeService.Models;
using System.Threading;
using System.Threading.Tasks;

namespace DateTimeService.DatabaseManagementNewServices.Interfaces
{
    public interface IDatabaseCheck
    {
        public Task<ElasticResponse> GetElasticLogsInformationAsync(string databaseConnectionWithOutCredentials, CancellationToken cancellationToken);

        public Task<bool> CheckAvailabilityAsync(string databaseConnectionString, CancellationToken cancellationToken, long executionLimit);

        public Task<bool> CheckAggregationsAsync(DatabaseInfo database, CancellationToken cancellationToken);


    }
}

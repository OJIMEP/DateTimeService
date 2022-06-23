using System.Threading;
using System.Threading.Tasks;

namespace DateTimeService.DatabaseManagementNewServices.Interfaces
{
    public interface IDatabaseAvailabilityControl
    {
        public Task CheckAndUpdateDatabasesStatus(CancellationToken cancellationToken);
    }
}

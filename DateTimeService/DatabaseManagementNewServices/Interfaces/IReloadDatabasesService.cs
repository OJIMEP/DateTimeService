using System.Threading;
using System.Threading.Tasks;

namespace DateTimeService.DatabaseManagementNewServices.Interfaces
{
    public interface IReloadDatabasesService
    {
        public Task ReloadAsync(CancellationToken cancellationToken);
    }
}

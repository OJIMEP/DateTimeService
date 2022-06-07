using System.Threading;

namespace DateTimeService.DatabaseManagementNewServices.Interfaces
{
    public interface IReloadDatabasesService
    {
        public void Reload(CancellationToken cancellationToken);
    }
}

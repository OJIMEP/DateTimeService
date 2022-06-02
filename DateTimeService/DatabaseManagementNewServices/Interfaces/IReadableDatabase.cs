using DateTimeService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DateTimeService.DatabaseManagementNewServices.Interfaces
{
    public interface IReadableDatabase
    {
        public List<DatabaseInfo> GetAllDatabases();
        public bool SynchronizeDatabasesListFromFile(List<DatabaseInfo> newDatabases);
        public bool AddDatabase(DatabaseInfo newDatabaseEntity);
        public bool DeleteDatabase(string connection);
        public bool UpdateDatabaseFromFile(DatabaseInfo newDatabaseEntity);
    }
}

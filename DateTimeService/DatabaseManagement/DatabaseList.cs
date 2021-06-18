using DateTimeService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DateTimeService.Data
{
    public static class DatabaseList
    {

        public static List<DatabaseInfo> databases { get; set; }

        static DatabaseList()
        {
            databases = new();
        }
        public static void CreateDatabases(List<DatabaseConnectionParameter> connectionParameters)
        {
            foreach (var item in connectionParameters)
            {
                var db = new DatabaseInfo();
                db.Connection = item.Connection;
                db.ConnectionWithoutCredentials = LoadBalancing.RemoveCredentialsFromConnectionString(db.Connection);
                db.Priority = item.Priority;
                db.Type = item.Type;

                databases.Add(db);
            }
        }
    }
}

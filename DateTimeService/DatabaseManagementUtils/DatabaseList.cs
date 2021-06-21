using DateTimeService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DateTimeService.Data
{
    public static class DatabaseList
    {

        public static List<DatabaseInfo> Databases { get; set; }
        public static DateTimeOffset LastReadFromFile { get; set; }

        public static readonly object locker = new();

        static DatabaseList()
        {
            Databases = new();
        }
        public static Task CreateUpdateDatabases(List<DatabaseConnectionParameter> connectionParameters)
        {

            if (LastReadFromFile == default || DateTimeOffset.Now - LastReadFromFile > TimeSpan.FromSeconds(5))
            {
                lock (locker)
                {
                    List<DatabaseInfo> databasesInFile = new();
                    List<DatabaseInfo> databasesToDelete = new();
                    foreach (var item in connectionParameters)
                    {
                        var db = new DatabaseInfo(item);

                        var currentItem = Databases.FirstOrDefault(s => s.Connection == db.Connection);

                        if (currentItem == default)
                        {
                            Databases.Add(db);
                            databasesInFile.Add(db);
                        }
                        else
                        {
                            currentItem.Priority = db.Priority;
                            databasesInFile.Add(currentItem);
                        }
                    }

                    foreach (var item in Databases)
                    {
                        if (!databasesInFile.Contains(item))
                        {
                            databasesToDelete.Add(item);
                        }
                    }

                    foreach (var item in databasesToDelete)
                    {
                        Databases.Remove(item);
                    }

                    LastReadFromFile = DateTimeOffset.Now;
                }
            }

            return Task.CompletedTask;
        }

        public static Task DisableDatabase(string connectionString)
        {
            lock (locker)
            {
                foreach (var item in Databases.Where(s=>s.Connection == connectionString))
                {
                    item.AvailableToUse = false;
                }
            }

            return Task.CompletedTask;
        }
    }
}

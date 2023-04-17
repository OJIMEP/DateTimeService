using DateTimeService.Controllers;
using DateTimeService.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DateTimeService.Data
{
    public static class DatabaseList
    {

        public static List<DatabaseInfo> Databases { get; set; }
        public static DateTimeOffset LastReadFromFile { get; set; }

        public static readonly object locker = new();

        public static string Enviroment { get; set; }

        static DatabaseList()
        {
            Databases = new();
        }
        public static Task CreateUpdateDatabases(List<DatabaseConnectionParameter> connectionParameters, ILogger<DateTimeController> _logger)
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
                            LogUpdatedChanges(db, _logger, "Added database");
                        }
                        else
                        {
                            if (currentItem.Priority != db.Priority)
                            {
                                LogUpdatedChanges(db, _logger, "Priority change");
                            }
                            if (currentItem.Type != db.Type)
                            {
                                LogUpdatedChanges(db, _logger, "Type change");
                            }
                            currentItem.Priority = db.Priority;
                            currentItem.Type = db.Type;
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
                        LogUpdatedChanges(item, _logger, "Deleted database");
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
                foreach (var item in Databases.Where(s => s.Connection == connectionString))
                {
                    item.AvailableToUse = false;
                }
            }

            return Task.CompletedTask;
        }

        public static void LogUpdatedChanges(DatabaseInfo database, ILogger<DateTimeController> _logger, string updateDesc)
        {
            var logElement = new ElasticLogElement
            {
                LoadBalancingExecution = 0,
                ErrorDescription = "Database list updated",
                Status = LogStatus.Ok,
                DatabaseConnection = database.ConnectionWithoutCredentials
            };

            logElement.AdditionalData.Add("UpdateCause", updateDesc);
            var logstringElement = JsonSerializer.Serialize(logElement);

            _logger.LogInformation(logstringElement);
        }
    }
}

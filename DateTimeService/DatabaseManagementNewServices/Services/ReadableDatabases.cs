using DateTimeService.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace DateTimeService.DatabaseManagementNewServices.Services
{
    public class ReadableDatabases : Interfaces.IReadableDatabase
    {
        private readonly ILogger<ReadableDatabases> _logger;

        private readonly ConcurrentDictionary<string, DatabaseInfo> dbDictionary = new();

        public ReadableDatabases(ILogger<ReadableDatabases> logger)
        {
            _logger = logger;
        }

        public bool AddDatabase(DatabaseInfo database)
        {
            bool addResult;
            try
            {
                addResult = dbDictionary.TryAdd(database.Connection, database);
                if (addResult)
                {
                    LogUpdatedChanges(database.ConnectionWithoutCredentials, "Added database", "");
                }
                else
                {
                    throw new Exception("Database already exists");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Adding database failed", ex);
                addResult = false;
            }

            return addResult;
        }

        public bool DeleteDatabase(string connection)
        {
            bool removeResult;

            try
            {
                removeResult = false;

                removeResult = dbDictionary.TryRemove(connection, out DatabaseInfo removedValue);
                if (removeResult)
                {
                    LogUpdatedChanges(removedValue.ConnectionWithoutCredentials, "Removed database", "");
                }
                else
                {
                    throw new Exception("Database not found");
                }
            }
            catch (Exception ex)
            {
                removeResult = false;
                _logger.LogWarning("Deleting database failed", ex);
            }

            return removeResult;
        }

        public List<DatabaseInfo> GetAllDatabases()
        {
            var result = dbDictionary.Values.ToList();

            return result;
        }

        public bool SynchronizeDatabasesListFromFile(List<DatabaseInfo> newDatabases)
        {
            bool result = true;

            var databasesToDelete = dbDictionary.Keys.Where(pldDbConnString => !newDatabases.Any(newDb => newDb.Connection == pldDbConnString)).ToList();

            foreach (var database in databasesToDelete)
            {
                result = result && DeleteDatabase(database);
            }

            foreach (var newDb in newDatabases)
            {
                if (dbDictionary.ContainsKey(newDb.Connection))
                {
                    result = result && UpdateDatabaseFromFile(newDb);
                }
                else
                {
                    result = result && AddDatabase(newDb);
                }
            }

            return result;
        }

        public bool UpdateDatabaseFromFile(DatabaseInfo newDatabaseEntity)
        {
            bool updateResult;
            try
            {
                var getResult = dbDictionary.TryGetValue(newDatabaseEntity.Connection, out DatabaseInfo currentDatabaseEntity);
                if (getResult)
                {
                    bool changedPriority = newDatabaseEntity.Priority != currentDatabaseEntity.Priority;
                    bool changedType = newDatabaseEntity.Type != currentDatabaseEntity.Type;

                    if (changedPriority || changedType)
                    {
                        var changedEntity = (DatabaseInfo)currentDatabaseEntity.Clone();
                        changedEntity.Priority = newDatabaseEntity.Priority;
                        changedEntity.Type = newDatabaseEntity.Type;
                        updateResult = dbDictionary.TryUpdate(newDatabaseEntity.Connection, changedEntity, currentDatabaseEntity);

                        if (updateResult)
                        {
                            if (changedPriority)
                            {
                                LogUpdatedChanges(newDatabaseEntity.ConnectionWithoutCredentials, "Priority changed", $"New priority = {newDatabaseEntity.Priority}");
                            }
                            if (changedType)
                            {
                                LogUpdatedChanges(newDatabaseEntity.ConnectionWithoutCredentials, "Priority changed", $"New priority = {newDatabaseEntity.Priority}");
                            }
                        }
                        else
                            throw new Exception("Database update failed");
                    }
                    else
                        updateResult = true;

                }
                else
                    throw new Exception("Database not found");

            }
            catch (Exception ex)
            {
                _logger.LogWarning("Updating database failed", ex);
                updateResult = false;
            }

            return updateResult;

        }

        private void LogUpdatedChanges(string connectionName, string description, string updateDesc, string status = "Ok")
        {
            var logElement = new ElasticLogElement
            {
                LoadBalancingExecution = 0,
                ErrorDescription = description,
                Status = status,
                DatabaseConnection = connectionName
            };

            logElement.AdditionalData.Add("UpdateCause", updateDesc);
            var logstringElement = JsonSerializer.Serialize(logElement);

            _logger.LogInformation(logstringElement);
        }
    }
}

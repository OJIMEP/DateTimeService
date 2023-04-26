using DateTimeService.Data;
using DateTimeService.DatabaseManagementUtils;
using System;
using System.Collections.Generic;

namespace DateTimeService.Models
{
    public class DatabaseInfo : DatabaseConnectionParameter, ICloneable
    {

        public string ConnectionWithoutCredentials { get; set; }

        public bool AvailableToUse { get; set; }
        public DateTimeOffset LastFreeProcCacheCommand { get; set; }
        public DateTimeOffset LastCheckAvailability { get; set; }
        public DateTimeOffset LastCheckAggregations { get; set; }
        public DateTimeOffset LastCheckPerfomance { get; set; }
        public int ActualPriority { get; set; }
        public bool ExistsInFile { get; set; }
        public bool CustomAggregationsAvailable { get; set; }
        public int CustomAggsFailCount { get; set; }
        public int TimeCriteriaFailCount { get; set; }
        public DatabaseType DatabaseType { get; set; }

        public DatabaseInfo(DatabaseConnectionParameter connectionParameter)
        {
            Connection = connectionParameter.Connection;
            ConnectionWithoutCredentials = LoadBalancing.RemoveCredentialsFromConnectionString(connectionParameter.Connection);
            Priority = connectionParameter.Priority;
            Type = connectionParameter.Type;
            ActualPriority = connectionParameter.Priority;
            DatabaseType = connectionParameter.Type switch
            {
                "main" => DatabaseType.Main,
                "replica_full" => DatabaseType.ReplicaFull,
                "replica_tables" => DatabaseType.ReplicaTables,
                _ => DatabaseType.Main
            };
        }

        public object Clone()
        {
            var result = new DatabaseInfo(this)
            {
                AvailableToUse = AvailableToUse,
                LastFreeProcCacheCommand = LastFreeProcCacheCommand,
                LastCheckAvailability = LastCheckAvailability,
                LastCheckAggregations = LastCheckAggregations,
                LastCheckPerfomance = LastCheckPerfomance,
                ActualPriority = ActualPriority,
                ExistsInFile = ExistsInFile,
                CustomAggregationsAvailable = CustomAggregationsAvailable,
                CustomAggsFailCount = CustomAggsFailCount,
                TimeCriteriaFailCount = TimeCriteriaFailCount,
                DatabaseType = DatabaseType
            };

            return result;

        }
    }


}

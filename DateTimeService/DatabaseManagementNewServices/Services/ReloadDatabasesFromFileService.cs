using DateTimeService.Data;
using DateTimeService.DatabaseManagementNewServices.Interfaces;
using DateTimeService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DateTimeService.DatabaseManagementNewServices.Services
{
    public class ReloadDatabasesFromFileService : IReloadDatabasesService
    {
        protected readonly IReadableDatabase _databasesService;
        private readonly ILogger<ReloadDatabasesFromFileService> _logger;
        private readonly IConfiguration _configuration;

        public ReloadDatabasesFromFileService(IReadableDatabase databases,
                                              ILogger<ReloadDatabasesFromFileService> logger,
                                              IConfiguration configuration)
        {
            _databasesService = databases;
            _logger = logger;
            _configuration = configuration;
        }

        public void Reload()
        {
            var dbList = _configuration.GetSection("OneSDatabases").Get<List<DatabaseConnectionParameter>>();

            if (dbList == null)
            {
                dbList = new();
            }

            List<DatabaseInfo> databases = dbList.Select(x => new DatabaseInfo(x)).ToList();

            var result = _databasesService.SynchronizeDatabasesListFromFile(databases);

            if (!result)
            {
                _logger.LogError("Database reloading from file failed!");
            }
        }
    }
}

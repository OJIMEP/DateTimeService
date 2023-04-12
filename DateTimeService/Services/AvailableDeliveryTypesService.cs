using DateTimeService.Models.AvailableDeliveryTypes;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using DateTimeService.Data;
using Microsoft.Extensions.Configuration;
using DateTimeService.Exceptions;
using System.Collections.Generic;

namespace DateTimeService.Services
{
    public class AvailableDeliveryTypesService : IAvailableDeliveryTypesService
    {
        private readonly IConfiguration _configuration;
        private readonly ILoadBalancing _loadBalacing;
       
        public AvailableDeliveryTypesService(IConfiguration configuration, ILoadBalancing loadBalancing)
        {
            _configuration = configuration;
            _loadBalacing = loadBalancing;
        }

        public async Task<ResponseAvailableDeliveryTypes> GetAvailableDeliveryTypes(RequestAvailableDeliveryTypes inputData)
        {
            string databaseType = "";
            bool customAggs = false;

            var result = new ResponseAvailableDeliveryTypes();

            SqlConnection conn;
            Stopwatch watch = new();
            watch.Start();
            try
            {
                var dbConnection = await _loadBalacing.GetDatabaseConnectionAsync();
                conn = dbConnection.Connection;
                databaseType = dbConnection.DatabaseType;
                customAggs = dbConnection.UseAggregations;
            }
            catch (Exception ex)
            {
                throw new DbConnectionNotFoundException(ex.Message);
            }
            watch.Stop();

            if (conn == null)
            {
                throw new DbConnectionNotFoundException("Не найдено доступное соединение к БД");
            }

            return result;
        }
    }
}

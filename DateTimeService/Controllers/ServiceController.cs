using AutoMapper;
using DateTimeService.Areas.Identity.Data;
using DateTimeService.Data;
using DateTimeService.DatabaseManagementNewServices.Interfaces;
using DateTimeService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DateTimeService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IReadableDatabase _readableDatabaseService;
        private readonly ILoadBalancing _loadBalacing;
        private readonly IMapper _mapper;

        public ServiceController(IReadableDatabase readableDatabaseService, ILoadBalancing loadBalacing, IMapper mapper)
        {
            _readableDatabaseService = readableDatabaseService;
            _loadBalacing = loadBalacing;
            _mapper = mapper;
        }

        [Route("HealthCheck")]
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> HealthCheckAsync()
        {

            SqlConnection conn;

            try
            {
                var dbConnection = await _loadBalacing.GetDatabaseConnectionAsync();
                conn = dbConnection.Connection;
            }
            catch
            {
                return StatusCode(500);
            }



            if (conn == null)
            {
                Dictionary<string, string> errorDesc = new();
                errorDesc.Add("ErrorDescription", "Не найдено доступное соединение к БД");

                return StatusCode(500, errorDesc);
            }

            await conn.CloseAsync();
            return StatusCode(200, new { Status = "Ok" });
        }

        [Route("Databases")]
        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> CurrenDBStatesAsync()
        {
            var dbList = _readableDatabaseService.GetAllDatabases().Select(x => _mapper.Map<ResponseDatabaseStatusList>(x)).ToList();

            return Ok(dbList);
        }

    }
}

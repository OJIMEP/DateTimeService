using DateTimeService.Controllers;
using DateTimeService.Data;
using DateTimeService.Logging;
using DateTimeService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DateTimeService.Services
{
    public class IntervalListDataService: IDataService<RequestIntervalList, ResponseIntervalList>
    {
        private readonly IConfiguration _configuration;
        private readonly ILoadBalancing _loadBalancing;
        private readonly ILogger<DateTimeController> _logger;

        public IntervalListDataService(IConfiguration configuration, ILoadBalancing loadBalancing, ILogger<DateTimeController> logger)
        {
            _configuration = configuration;
            _loadBalancing = loadBalancing;
            _logger = logger;
        }

        public IServiceLogElement GetLog()
        {
            throw new System.NotImplementedException();
            //return logInternal;
        }

        public Task<ResponseIntervalList> GetDataByParam(RequestIntervalList inputData)
        {
            throw new System.NotImplementedException();
        }
    }
}

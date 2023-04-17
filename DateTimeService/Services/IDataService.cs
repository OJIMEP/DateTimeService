using DateTimeService.Logging;
using System.Threading.Tasks;

namespace DateTimeService.Services
{
    public interface IDataService<in In, Out>
    {
        Task<Out> GetDataByParam(In inputData);

        IServiceLogElement GetLog();
    }
}

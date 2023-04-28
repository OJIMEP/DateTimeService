using DateTimeService.Models;
using DateTimeService.Models.AvailableDeliveryTypes;
using System.Threading;
using System.Threading.Tasks;

namespace DateTimeService.Services
{
    public interface IDataService
    {
        Task<ResponseAvailableDeliveryTypes> GetAvailableDeliveryTypes(RequestAvailableDeliveryTypes inputData, CancellationToken token = default);

        Task<ResponseIntervalList> GetIntervalList(RequestIntervalList inputData, CancellationToken token = default);
    }
}

using DateTimeService.Logging;
using DateTimeService.Models.AvailableDeliveryTypes;
using System.Threading.Tasks;

namespace DateTimeService.Services
{
    public interface IAvailableDeliveryTypesService
    {
        Task<ResponseAvailableDeliveryTypes> GetAvailableDeliveryTypes(RequestAvailableDeliveryTypes inputData);

        LogElementInternal GetLog();
    }
}

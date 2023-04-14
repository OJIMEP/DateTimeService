using System.Text.Json.Serialization;

namespace DateTimeService.Models.AvailableDeliveryTypes
{
    public class AvailableDeliveryTypeError
    {
        [JsonPropertyName("courier")]
        public DeliveryTypeErrorMessage Courier { get; set; }

        [JsonPropertyName("pickup_point")]
        public DeliveryTypeErrorMessage Self { get; set; }

        [JsonPropertyName("interval")]
        public DeliveryTypeErrorMessage YourTime { get; set; }

        public AvailableDeliveryTypeError()
        {
            Courier = new();
            Self = new();
            YourTime = new();
        }
    }

    public class DeliveryTypeErrorMessage
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace DateTimeService.Models.AvailableDeliveryTypes
{
    public class ResponseAvailableDeliveryTypes
    {
        [JsonPropertyName("courier")]
        public DeliveryTypeAvailability Courier { get; set; }

        [JsonPropertyName("pickup_point")]
        public DeliveryTypeAvailability Self { get; set; }

        [JsonPropertyName("interval")]
        public DeliveryTypeAvailability YourTime { get; set; }

        public ResponseAvailableDeliveryTypes()
        {
            Courier = new();
            Self = new();
            YourTime = new();
        }
    }

    public class DeliveryTypeAvailability
    {
        [JsonPropertyName("available")]
        public bool IsAvailable { get; set; }
    }
}

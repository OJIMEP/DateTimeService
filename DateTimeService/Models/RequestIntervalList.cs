using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DateTimeService.Models
{
    public class RequestIntervalListDTO
    {
        [JsonPropertyName("address_id")]
        public string AddressId { get; set; }

        [JsonPropertyName("delivery_type")]
        public string DeliveryType { get; set; }

        [JsonPropertyName("pickup_point")]
        public string PickupPoint { get; set; }

        [JsonPropertyName("floor")]
        public double? Floor { get; set; }

        [JsonPropertyName("payment")]
        public string Payment { get; set; }

        [JsonPropertyName("order_items")]
        public List<RequestDataCodeItemDTO> OrderItems { get; set; }
    }

    public class RequestIntervalList
    {
        [JsonPropertyName("address_id")]
        public string AddressId { get; set; }

        [JsonPropertyName("delivery_type")]
        public string DeliveryType { get; set; }

        [JsonPropertyName("pickup_point")]
        public string PickupPoint { get; set; }

        [JsonPropertyName("floor")]
        public double? Floor { get; set; }

        [JsonPropertyName("payment")]
        public string Payment { get; set; }

        [JsonPropertyName("order_items")]
        public List<RequestDataCodeItem> OrderItems { get; set; }
    }
}

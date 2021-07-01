using System.Text.Json.Serialization;

namespace DateTimeService.Models
{
    public class RequestDataCodeItemDTO
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("sales_code")]
        public string SalesCode { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("pickup_points")]
        public string[] PickupPoints { get; set; }
    }
    public class RequestDataCodeItem
    {
        [JsonPropertyName("article")]
        public string Article { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("sales_code")]
        public string SalesCode { get; set; }  // код с сайта без префиксов и нулей

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        [JsonPropertyName("pickup_points")]
        public string[] PickupPoints { get; set; }

        public RequestDataCodeItem()
        {
            PickupPoints = System.Array.Empty<string>();
        }
    }
}
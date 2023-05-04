using System;
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
        public string Article { get; set; }
        public string Code { get; set; }
        public string SalesCode { get; set; }  // код с сайта без префиксов и нулей
        public int Quantity { get; set; }
        public string CacheKey { get; set; }
        public string[] PickupPoints { get; set; }

        public RequestDataCodeItem()
        {
            PickupPoints = Array.Empty<string>();
        }
    }
}
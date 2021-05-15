using System.Text.Json.Serialization;

namespace DateTimeService.Models
{
    public class RequestDataCodeItemDTO
    {
        public string code { get; set; }
        public string sales_code { get; set; }
        public int quantity { get; set; }
        [JsonPropertyName("pickup_points")]
        public string[] PickupPoints { get; set; }
    }
    public class RequestDataCodeItem
    {
        public string article { get; set; }
        public string code { get; set; }
        public string sales_code { get; set; }  // код с сайта без префиксов и нулей
        public int quantity { get; set; }
        [JsonPropertyName("pickup_points")]
        public string[] PickupPoints { get; set; }

        public RequestDataCodeItem()
        {
            PickupPoints = new string[0];
        }
    }
}
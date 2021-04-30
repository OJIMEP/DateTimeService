using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DateTimeService.Models
{
    public class RequestIntervalListDTO
    {
        public string address_id { get; set; }
        public string delivery_type { get; set; }
        public double? floor { get; set; }
        public string payment { get; set; }
        [JsonPropertyName("order_items")]
        public List<RequestOrderItemsDTO> orderItems { get; set; }
    }

    public class RequestOrderItemsDTO
    {
        public string code { get; set; }
        public string sale_code { get; set; }
        public int quantity { get; set; }
    }

    public class RequestIntervalList
    {
        public string address_id { get; set; }
        public string delivery_type { get; set; }
        public double? floor { get; set; }
        public string payment { get; set; }
        public List<RequestOrderItems> orderItems { get; set; }
    }

    public class RequestOrderItems
    {
        public string partNumber { get; set; }
        public string code { get; set; }
        public int quantity { get; set; }
    }
}

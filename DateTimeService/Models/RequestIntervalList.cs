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
        public List<RequestDataCodeItemDTO> orderItems { get; set; }
    }

    public class RequestIntervalList
    {
        public string address_id { get; set; }
        public string delivery_type { get; set; }
        public double? floor { get; set; }
        public string payment { get; set; }
        public List<RequestDataCodeItem> orderItems { get; set; }
    }
}

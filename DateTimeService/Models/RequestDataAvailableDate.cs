using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DateTimeService.Models
{
    public class RequestDataAvailableDate
    {
        public string city_id { get; set; }
        public enum delivery_types { self, courier, both }
        public string[] codes { get; set; }

    }
}

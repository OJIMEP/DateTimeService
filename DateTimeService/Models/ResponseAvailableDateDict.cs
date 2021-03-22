using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DateTimeService.Models
{
    public class ResponseAvailableDateDict
    {
        public Dictionary<string, ResponseAvailableDateDictElement> data { get; set; }

        public ResponseAvailableDateDict()
        {
            data = new Dictionary<string, ResponseAvailableDateDictElement>();
        }
    }

    public class ResponseAvailableDateDictElement
    {
        public string code { get; set; }
        public DateTime courier { get; set; }
        public DateTime self { get; set; }

    }
}

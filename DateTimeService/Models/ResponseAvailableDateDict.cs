using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
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
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string courier { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string self { get; set; }
    }


}

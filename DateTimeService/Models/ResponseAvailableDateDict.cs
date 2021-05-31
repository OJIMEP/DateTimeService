using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DateTimeService.Models
{
    public class ResponseAvailableDateDict
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Dictionary<string, ResponseAvailableDateDictElement> data { get; set; }
        [JsonPropertyName("data")]
        public Dictionary<string, Dictionary<string,string>> data1 { get; set; }


        public ResponseAvailableDateDict()
        {
            data = new Dictionary<string, ResponseAvailableDateDictElement>();
            data1 = new Dictionary<string, Dictionary<string,string>>();
        }
    }

    public class ResponseAvailableDateDictElement
    {
        public string code { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string sales_code { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string courier { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string self { get; set; }
    }

}

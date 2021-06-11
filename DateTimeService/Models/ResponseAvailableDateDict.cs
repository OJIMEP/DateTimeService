using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DateTimeService.Models
{
    public class ResponseAvailableDateDict
    {
        [JsonPropertyName("data")]
        public Dictionary<string, ResponseAvailableDateDictElement> Data { get; set; }


        public ResponseAvailableDateDict()
        {
            Data = new Dictionary<string, ResponseAvailableDateDictElement>();
        }
    }

    public class ResponseAvailableDateDictElement
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull), JsonPropertyName("sales_code")]
        public string SalesCode { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull), JsonPropertyName("courier")]
        public string Courier { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull), JsonPropertyName("self")]
        public string Self { get; set; }
    }

    public class ResponseAvailableDateDictBothDates
    {
        [JsonPropertyName("data")]
        public Dictionary<string, ResponseAvailableDateDictElementBothDates> Data { get; set; }
       
        public ResponseAvailableDateDictBothDates()
        {
            Data = new Dictionary<string, ResponseAvailableDateDictElementBothDates>();
        }
    }

    public class ResponseAvailableDateDictElementBothDates
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull), JsonPropertyName("sales_code")]
        public string SalesCode { get; set; }
        
        [JsonPropertyName("courier")]
        public string Courier { get; set; }

        [JsonPropertyName("self")]
        public string Self { get; set; }
    }

}

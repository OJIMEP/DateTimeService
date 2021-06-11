using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DateTimeService.Models
{
    public class ResponseAvailableDate
    {
        [JsonPropertyName("article")]
        public List<string> Article { get; set; }

        [JsonPropertyName("code")]
        public List<string> Code { get; set; }

        [JsonPropertyName("courier")]
        public List<DateTimeOffset> Courier { get; set; }

        [JsonPropertyName("self")]
        public List<DateTimeOffset> Self { get; set; }

        public ResponseAvailableDate()
        {
            Article = new List<string>();
            Code = new List<string>();
            Courier = new List<DateTimeOffset>();
            Self = new List<DateTimeOffset>();
        }


    }
}

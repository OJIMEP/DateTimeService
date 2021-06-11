using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DateTimeService.Logging
{
    public class ElasticLogMessage
    {
        [JsonPropertyName("message")]
        public List<string> Message { get; set; }

        public ElasticLogMessage()
        {
            Message = new List<string>();
        }
    }
}

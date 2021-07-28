using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DateTimeService.Models
{
    public class ResponseIntervalList
    {
        [JsonPropertyName("data")]
        public List<ResponseIntervalListElement> Data { get; set; }

        public ResponseIntervalList()
        {
            Data = new List<ResponseIntervalListElement>();
        }

    }

    public class ResponseIntervalListWithOffSet
    {
        [JsonPropertyName("data")]
        public List<ResponseIntervalListElementWithOffSet> Data { get; set; }

        public ResponseIntervalListWithOffSet()
        {
            Data = new List<ResponseIntervalListElementWithOffSet>();
        }

    }

    public class ResponseIntervalListElement
    {
        [JsonPropertyName("begin")]
        public DateTime Begin { get; set; }

        [JsonPropertyName("end")]
        public DateTime End { get; set; }
        [JsonPropertyName("bonus")]
        public bool Bonus { get; set; }
    }

    public class ResponseIntervalListElementWithOffSet
    {
        [JsonPropertyName("begin")]
        public DateTimeOffset Begin { get; set; }

        [JsonPropertyName("end")]
        public DateTimeOffset End { get; set; }
        [JsonPropertyName("bonus")]
        public bool Bonus { get; set; }
    }
}

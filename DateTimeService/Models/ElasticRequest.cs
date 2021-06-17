using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DateTimeService.Models
{
    public class ElasticRequest
    {
        public ElasticRequest()
        {
            Query = new();
            Aggregations = new();
        }

        [JsonPropertyName("query"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public QueryClass Query { get; set; }

        [JsonPropertyName("aggs"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, AggregationClass> Aggregations { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

    }

    public class QueryClass
    {
        [JsonPropertyName("range")]
        public Dictionary<string, object> Range { get; set; }

        public QueryClass()
        {
            Range = new();
        }
    }

    public class AggregationClass
    {
        [JsonPropertyName("terms"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ConditionsClass Terms { get; set; }

        [JsonPropertyName("percentiles"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ConditionsClass Percentiles { get; set; }

        [JsonPropertyName("avg"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ConditionsClass Avg { get; set; }

        [JsonPropertyName("aggs"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, AggregationClass> Aggregations { get; set; }


    }

    public class ConditionsClass
    {
        [JsonPropertyName("field")]
        public string Field { get; set; }

        [JsonPropertyName("size"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Int32 Size { get; set; }

        [JsonPropertyName("percents"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double[] Percents { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DateTimeService.Models
{
    public class ElasticResponse
    {
        [JsonPropertyName("took")]
        public int Took { get; set; }

        [JsonPropertyName("timed_out")]
        public bool TimedOut { get; set; }

        [JsonPropertyName("_shards")]
        public Shards Shards { get; set; }

        
        [JsonPropertyName("aggregations")]
        public Dictionary<string, Aggregations> Aggregations { get; set; }
        public ElasticResponse()
        {
            Aggregations = new();
        }


    }

    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class Shards
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("successful")]
        public int Successful { get; set; }

        [JsonPropertyName("skipped")]
        public int Skipped { get; set; }

        [JsonPropertyName("failed")]
        public int Failed { get; set; }
    }

    public class Total
    {
        [JsonPropertyName("value")]
        public int Value { get; set; }

        [JsonPropertyName("relation")]
        public string Relation { get; set; }
    }

    


    public class Aggregations
    {
        [JsonPropertyName("doc_count_error_upper_bound")]
        public int DocCountErrorUpperBound { get; set; }

        [JsonPropertyName("sum_other_doc_count")]
        public int SumOtherDocCount { get; set; }

        [JsonPropertyName("buckets")]
        public List<BucketClass> Buckets { get; set; }

        public Aggregations()
        {
            Buckets = new();
        }
    }

    public class BucketClass
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }
        
        [JsonPropertyName("doc_count")]
        public int DocCount { get; set; }

        [JsonPropertyName("week_avg")]
        public AggValues WeekAvg { get; set; }

        [JsonPropertyName("time_percentile")]
        public AggValues TimePercentile { get; set; }

        [JsonPropertyName("load_bal")]
        public AggValues LoadBalance { get; set; }
    }

    public class AggValues
    {
        [JsonPropertyName("value")]
        public double Value { get; set; }
        
        [JsonPropertyName("values")]
        public Dictionary<string,double> Values { get; set; }

    }

}

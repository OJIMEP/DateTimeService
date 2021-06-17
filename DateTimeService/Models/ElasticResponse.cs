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
        public Aggregations Aggregations { get; set; }
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

    

    public class Values
    {
        [JsonPropertyName("95.0")]
        public double _950 { get; set; }

        [JsonPropertyName("99.0")]
        public double _990 { get; set; }

        [JsonPropertyName("99.9")]
        public double _999 { get; set; }
    }

    public class LoadTimeOutlierResp
    {
        [JsonPropertyName("values")]
        public Values Values { get; set; }
    }

    public class WeekAvgResp
    {
        [JsonPropertyName("value")]
        public double Value { get; set; }
    }

    public class Aggregations
    {
        [JsonPropertyName("load_time_outlier")]
        public LoadTimeOutlierResp LoadTimeOutlier { get; set; }

        [JsonPropertyName("week_avg")]
        public WeekAvgResp WeekAvg { get; set; }
    }

   

}

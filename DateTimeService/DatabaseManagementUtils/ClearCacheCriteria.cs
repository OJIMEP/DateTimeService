namespace DateTimeService.DatabaseManagementUtils
{
    public class ClearCacheCriteria
    {
        public string CriteriaType { get; set; } //RecordCount, MaximumResponseTime
        public float Percentile_95 { get; set; }
        public float RecordCountBegin { get; set; } //в минутах
        public float RecordCountEnd { get; set; }
        public float LoadBalance { get; set; }

    }
}

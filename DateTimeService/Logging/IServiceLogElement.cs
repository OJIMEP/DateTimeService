namespace DateTimeService.Logging
{
    public interface IServiceLogElement
    {     
        public LogStatus Status { get; set; }       
        public long LoadBalancingExecution { get; }
        public long TimeSqlExecutionFact { get; }
        public long TimeFullExecution { get; set; }
        public string? ErrorDescription { get; }
    }
}

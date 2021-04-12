namespace DateTimeService
{
    public class ElasticLogElement
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public string Host { get; set; }
        public string ResponseContent { get; set; }
        public string RequestContent { get; set; }
        public long TimeSQLExecution { get; set; }
        public string Status { get; set; }
        public string ErrorDescription { get; set; }
        public long TimeFullExecution { get; set; }
        public string DatabaseConnection { get; set; }
        public string AuthenticatedUser { get; set; }
    }
}

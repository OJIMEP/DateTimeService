using DateTimeService.Data;
using DateTimeService.Logging;
using System.Collections.Generic;

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
        public long TimeSQLExecutionFact { get; set; }
        public LogStatus Status { get; set; }
        public string ErrorDescription { get; set; }
        public long TimeFullExecution { get; set; }
        public string DatabaseConnection { get; set; }
        public string AuthenticatedUser { get; set; }
        public long TimeBtsExecution { get; set; }
        public long TimeLocationExecution { get; set; }
        public long LoadBalancingExecution { get; set; }
        public long GlobalParametersExecution { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }
        public string Enviroment { get; set; }
        public string ServiceName { get; set; }

        public ElasticLogElement()
        {
            Enviroment = DatabaseList.Enviroment ?? "Unset";
            AdditionalData = new();
            ServiceName = "DateTime";
        }

        // функция заполняет одноименные поля текущего объекта из параметра типа ILogElementInternal
        public void FillFromLogElementInternal(IServiceLogElement logElement)
        {
            TimeSQLExecution = logElement.TimeSqlExecutionFact;
            TimeFullExecution = logElement.TimeFullExecution;
            Status = logElement.Status;
            ErrorDescription = logElement.ErrorDescription;
            LoadBalancingExecution = logElement.LoadBalancingExecution;
        }
    }
}

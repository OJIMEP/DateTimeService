using DateTimeService.Data;
using DateTimeService.Models.AvailableDeliveryTypes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace DateTimeService.Logging
{
    public class LogAvailableDeliveryTypes: IServiceLogElement
    {
        public List<long> TimeSqlExecutionFacts { get; set; }
        public long TimeFullExecution { get; set; }
        public LogStatus Status { get; set; }
        public AvailableDeliveryTypeError ErrorDescriptions { get; set; }
        public List<string> DatabaseConnections { get; set; }
        public List<long> DbConnectTimesInMilliseconds { get; set; }
        public Dictionary<string, string?> AdditionalData { get; set; }

        public long LoadBalancingExecution { get
            {
                if (DbConnectTimesInMilliseconds.Count == 0) return 0;
                return (long)DbConnectTimesInMilliseconds.Average();
            }
        }

        public long TimeSqlExecutionFact
        {
            get
            {
                if (TimeSqlExecutionFacts.Count == 0) return 0;
                return (long)TimeSqlExecutionFacts.Average();
            }
        }

        public string ErrorDescription { get
            {
                return JsonSerializer.Serialize(ErrorDescriptions);
            }
        }

        public LogAvailableDeliveryTypes()
        {
            AdditionalData = new();
            ErrorDescriptions = new();
            TimeSqlExecutionFacts = new();
            DatabaseConnections = new();
            DbConnectTimesInMilliseconds = new();
            Status = LogStatus.Ok;
        }

        public void AddError(string deliveryType, string errorDescription)
        {
            Status = LogStatus.Error;

            if (deliveryType == Constants.Self) { ErrorDescriptions.Self.Error = errorDescription; }
            if (deliveryType == Constants.CourierDelivery) { ErrorDescriptions.Courier.Error = errorDescription; }
            if (deliveryType == Constants.YourTimeDelivery) { ErrorDescriptions.YourTime.Error = errorDescription; }
        }

        public void AddStatistics(IDictionary stats)
        {
            AdditionalData.TryAdd("stats", JsonSerializer.Serialize(stats));
        }

        public void AddExecutionFact(long elapsedMilliseconds)
        {
            TimeSqlExecutionFacts.Add(elapsedMilliseconds);
        }

        public void AddDatabaseConnection(string connectionString)
        {
            DatabaseConnections.Add(connectionString);
        }

        public void AddDbConnectTime(long elapsedMilliseconds)
        {
            DbConnectTimesInMilliseconds.Add(elapsedMilliseconds);
        }
    }
}

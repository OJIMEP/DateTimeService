using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DateTimeService.Logging
{
    public class ElasticLogMessage
    {
        public List<string> message { get; set; }

        public ElasticLogMessage()
        {
            message = new List<string>();
        }
    }
}

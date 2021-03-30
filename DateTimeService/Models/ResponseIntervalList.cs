using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DateTimeService.Models
{
    public class ResponseIntervalList
    {
        public List<ResponseIntervalListElement> data { get; set; }

        public ResponseIntervalList()
        {
            data = new List<ResponseIntervalListElement>();
        }

    }

    public class ResponseIntervalListElement
    {
        public DateTime begin { get; set; }
        public DateTime end { get; set; }
    }
}

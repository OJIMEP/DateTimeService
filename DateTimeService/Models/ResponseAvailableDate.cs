using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DateTimeService.Models
{
    public class ResponseAvailableDate
    {
        public List<string> code { get; set; }
        public List<DateTime> courier { get; set; }
        public List<DateTime> self { get; set; }

        public ResponseAvailableDate()
        {
            code = new List<string>();
            courier = new List<DateTime>();
            self = new List<DateTime>();
        }


    }
}

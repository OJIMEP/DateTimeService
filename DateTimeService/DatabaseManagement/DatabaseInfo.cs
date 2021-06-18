using DateTimeService.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DateTimeService.Models
{
    public class DatabaseInfo
    {
        public string Connection { get; set; }
        public int Priority { get; set; }
        public string Type { get; set; } //main, replica_full, replica_tables 

        public string ConnectionWithoutCredentials { get; set; }

        public bool AvailableToUse { get; set; }
        public DateTimeOffset LastFreeProcCacheCommand { get; set; }
        public DateTimeOffset LastCheckAvailability { get; set; }
        public DateTimeOffset LastCheckPerfomance { get; set; }
    }


}

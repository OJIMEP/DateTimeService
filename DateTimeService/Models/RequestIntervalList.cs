﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DateTimeService.Models
{
    public class RequestIntervalList
    {
        public string address_id { get; set; }
        public string[] delivery_type { get; set; }
        public string floor { get; set; }
        public List<RequestOrderItems> orderItems { get; set; }
    }

    public class RequestOrderItems
    {
        public string code { get; set; }
        public int quantity { get; set; }
    }
}
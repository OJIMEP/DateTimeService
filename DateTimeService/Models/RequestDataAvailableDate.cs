﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DateTimeService.Models
{
    public class RequestDataAvailableDate
    {
        [Required]
        public string city_id { get; set; }
        [Required]
        public string[] delivery_types { get; set; }
        [Required]
        public string[] codes { get; set; }

    }
}

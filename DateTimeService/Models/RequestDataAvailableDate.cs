using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DateTimeService.Models
{
    public class RequestDataAvailableDateDTO
    {
        [Required]
        public string city_id { get; set; }
        [Required]
        public string[] delivery_types { get; set; }
        [Required, MinLength(1)]
        public RequestDataCodeItemDTO[] codes { get; set; }

    }

    public class RequestDataAvailableDate
    {
        [Required]
        public string city_id { get; set; }
        [Required]
        public string[] delivery_types { get; set; }
        [Required, MinLength(1)]
        public RequestDataCodeItem[] codes { get; set; }

    }
}

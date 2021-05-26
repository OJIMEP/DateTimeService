using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DateTimeService.Models
{
    public class RequestDataAvailableDateByCodesDTO
    {
        [Required]
        public string city_id { get; set; }
        [Required]
        public string[] delivery_types { get; set; }
        [Required]
        public string[] codes { get; set; }

    }

    public class RequestDataAvailableDateByCodeItemsDTO
    {
        [Required]
        public string city_id { get; set; }
        [Required]
        public string[] delivery_types { get; set; }
        [Required, MinLength(1),JsonPropertyName("codes")]
        public RequestDataCodeItemDTO[] codeItems { get; set; }

    }

    public class RequestDataAvailableDate
    {
        [Required]
        public string city_id { get; set; }
        [Required]
        public string[] delivery_types { get; set; }
        [Required, MinLength(1), JsonPropertyName("codeItems")]
        public RequestDataCodeItem[] codes { get; set; }

        public RequestDataAvailableDate()
        {
            codes = Array.Empty<RequestDataCodeItem>();
        }
    }
}

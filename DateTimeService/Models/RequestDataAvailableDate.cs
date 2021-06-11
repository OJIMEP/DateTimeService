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
        [Required, JsonPropertyName("city_id")]
        public string CityId { get; set; }

        [Required, JsonPropertyName("delivery_types")]
        public string[] DeliveryTypes { get; set; }

        [Required, JsonPropertyName("codes")]
        public string[] Codes { get; set; }

    }

    public class RequestDataAvailableDateByCodeItemsDTO
    {
        [Required, JsonPropertyName("city_id")]
        public string CityId { get; set; }

        [Required, JsonPropertyName("delivery_types")]
        public string[] DeliveryTypes { get; set; }

        [Required, MinLength(1),JsonPropertyName("codes")]
        public RequestDataCodeItemDTO[] CodeItems { get; set; }

    }

    public class RequestDataAvailableDate
    {
        [Required, JsonPropertyName("city_id")]
        public string CityId { get; set; }

        [Required, JsonPropertyName("delivery_types")]
        public string[] DeliveryTypes { get; set; }

        [Required, MinLength(1), JsonPropertyName("codeItems")]
        public RequestDataCodeItem[] Codes { get; set; }

        public RequestDataAvailableDate()
        {
            Codes = Array.Empty<RequestDataCodeItem>();
        }
    }
}

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

        [Required, MinLength(1), JsonPropertyName("codes")]
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

        public RequestDataAvailableDate(bool fillData)
        {
            if (!fillData)
            {
                return;
            }

            CityId = "17030";
            DeliveryTypes = new string[] { "courier", "self" };
            var codesList = @"5684713, 5888304, 5820023, 5820020, 1095503, 5674906, 375559, 375561, 13775, 
                            5896679, 375560, 6291525, 13773, 13774, 6291513, 798732, 5807722, 5896606, 375554, 
                            375552, 5896563, 715431, 29495, 5915459, 6291529, 6029958, 6492972, 604836, 
                            5896691, 645932, 5896687, 623892, 673875, 86148, 379315, 379314, 514331, 
                            623899, 5896682, 441772, 87373, 86803, 5861320, 963086, 5805546, 86147, 
                            604815, 645930, 379304, 337651, 30695, 30694, 46223, 5896573, 30693, 1161502, 
                            616583, 5902601, 116731, 1161504".Split(",");
            List<RequestDataCodeItem> items = new();
            foreach (var item in codesList)
            {
                RequestDataCodeItem itemDTO = new();
                itemDTO.Article = item;
                itemDTO.PickupPoints = new string[] { "340", "388", "460", "417", "234", "2" };
                items.Add(itemDTO);
            }
            Codes = items.ToArray();
        }
    }
}

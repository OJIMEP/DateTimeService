using System.Text.Json.Serialization;
using System.Collections.Generic;
using System;

namespace DateTimeService.Models.AvailableDeliveryTypes
{
    public class RequestAvailableDeliveryTypesDTO
    {
        [JsonPropertyName("city_id")]
        public string CityId { get; set; }

        [JsonPropertyName("pickup_points")]
        public string[] PickupPoints { get; set; }

        [JsonPropertyName("items")]
        public List<RequestAvailableDeliveryTypesItemDTO> OrderItems { get; set; }
    }

    public class RequestAvailableDeliveryTypesItemDTO
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("sales_code")]
        public string SalesCode { get; set; }

        [JsonPropertyName("count")]
        public int Quantity { get; set; }
    }

    public class RequestAvailableDeliveryTypes
    {
        public string CityId { get; set; }

        public string[] PickupPoints { get; set; }

        public List<RequestAvailableDeliveryTypesItem> OrderItems { get; set; }

        public Dictionary<string, string> LogicalCheckInputData()
        {
            var errors = new Dictionary<string, string>();

            //if (String.IsNullOrEmpty(CityId))
            //{
            //    errors.Add("city_id", "Должен быть указан код города");
            //}
            //if (PickupPoints.Length == 0)
            //{
            //    errors.Add("pickup_points", "Должен быть указан хоть один пункт самовывоза");
            //}
            //if (OrderItems.Count == 0)
            //{
            //    errors.Add("items", "Должен быть указан хоть один товар");
            //}

            //foreach (var item in OrderItems)
            //{
            //    if (item.Quantity == 0)
            //    {
            //        errors.Add("count", "Количество товара не может быть 0");
            //    }

            //    if (item.SalesCode != null && item.SalesCode.Trim() == "" && !errors.ContainsKey("sales_code"))
            //    {
            //        errors.Add("sales_code", "Поле уценки не должно быть пустой строкой - либо заполнено, либо поле в принципе отсутствует");
            //    }
            //}

            return errors;
        }
    }

    public class RequestAvailableDeliveryTypesItem
    {
        public string Article { get; set; }

        public string Code { get; set; }

        public string SalesCode { get; set; }

        public int Quantity { get; set; }
    }
}

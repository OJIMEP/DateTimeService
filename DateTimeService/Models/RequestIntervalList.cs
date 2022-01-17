using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DateTimeService.Models
{
    public class RequestIntervalListDTO
    {
        [JsonPropertyName("address_id")]
        public string AddressId { get; set; }

        [JsonPropertyName("delivery_type")]
        public string DeliveryType { get; set; }

        [JsonPropertyName("pickup_point")]
        public string PickupPoint { get; set; }

        [JsonPropertyName("floor")]
        public double? Floor { get; set; }

        [JsonPropertyName("payment")]
        public string Payment { get; set; }

        [JsonPropertyName("order_number")]
        public string OrderNumber { get; set; }

        [JsonPropertyName("order_date")]
        public DateTime OrderDate { get; set; }

        [JsonPropertyName("x_coordinate")]
        public string Xcoordinate { get; set; }

        [JsonPropertyName("y_coordinate")]
        public string Ycoordinate { get; set; }

        [JsonPropertyName("order_items")]
        public List<RequestDataCodeItemDTO> OrderItems { get; set; }
    }

    public class RequestIntervalList
    {
        [JsonPropertyName("address_id")]
        public string AddressId { get; set; }

        [JsonPropertyName("delivery_type")]
        public string DeliveryType { get; set; }

        [JsonPropertyName("pickup_point")]
        public string PickupPoint { get; set; }

        [JsonPropertyName("floor")]
        public double? Floor { get; set; }

        [JsonPropertyName("payment")]
        public string Payment { get; set; }

        [JsonPropertyName("order_number")]
        public string OrderNumber { get; set; }

        [JsonPropertyName("order_date")]
        public DateTime OrderDate { get; set; }

        [JsonPropertyName("x_coordinate")]
        public string Xcoordinate { get; set; }

        [JsonPropertyName("y_coordinate")]
        public string Ycoordinate { get; set; }


        [JsonPropertyName("order_items")]
        public List<RequestDataCodeItem> OrderItems { get; set; }


        public Dictionary<string,string> LogicalCheckInputData()
        {
            var errors = new Dictionary<string, string>();

            if (String.IsNullOrEmpty(DeliveryType) && String.IsNullOrEmpty(OrderNumber))
            {
                errors.Add("delivery_type", "Должен быть указан тип доставки или номер имеющегося заказа");
            }

            if (DeliveryType == "courier")
            {
                if (String.IsNullOrEmpty(AddressId) && (String.IsNullOrEmpty(Xcoordinate) && String.IsNullOrEmpty(Ycoordinate)))
                {
                    errors.Add("address_id", "При курьерской доставке должен быть заполнен код адреса или координаты");
                }
                if (String.IsNullOrEmpty(Xcoordinate) && !String.IsNullOrEmpty(Ycoordinate))
                {
                    errors.Add("x_coordinate", "Обе координаты должны быть заполнены");
                }
                if (String.IsNullOrEmpty(Ycoordinate) && !String.IsNullOrEmpty(Xcoordinate))
                {
                    errors.Add("y_coordinate", "Обе координаты должны быть заполнены");
                }
                if (!String.IsNullOrEmpty(PickupPoint))
                {
                    errors.Add("pickup_point", "При курьерской доставке код ПВЗ должен отсутствовать");
                }
                if (!String.IsNullOrEmpty(OrderNumber))
                {
                    errors.Add("order_number", "При курьерской доставке номер заказа должен отсутствовать");
                }
                if (OrderDate != default)
                {
                    errors.Add("order_date", "При курьерской доставке дата заказа должна отсутствовать");
                }
            }
            if (DeliveryType == "self")
            {
                if (!String.IsNullOrEmpty(AddressId))
                {
                    errors.Add("address_id", "При самовывозе код адреса должен отсутствовать");
                }
                if (Floor != null)
                {
                    errors.Add("floor", "При самовывозе этаж должен отсутствовать");
                }
                if (String.IsNullOrEmpty(PickupPoint))
                {
                    errors.Add("pickup_point", "При самовывозе код ПВЗ должен быть заполнен");
                }
                if (!String.IsNullOrEmpty(OrderNumber))
                {
                    errors.Add("order_number", "При самовывозе номер заказа должен отсутствовать");
                }
                if (OrderDate != default)
                {
                    errors.Add("order_date", "При самовывозе дата заказа должна отсутствовать");
                }
            }

            if (!String.IsNullOrEmpty(OrderNumber)|| OrderDate != default)
            {
                if (String.IsNullOrEmpty(OrderNumber))
                {
                    errors.Add("order_number", "При указании времени заказа, должен быть указан и номер");
                }
                if (OrderDate == default)
                {
                    errors.Add("order_date", "При указании номера заказа, должна быть указана и дата");
                }

                if (!String.IsNullOrEmpty(AddressId))
                {
                    errors.Add("address_id", "При указании имеющегося заказа код адреса должен отсутствовать");
                }
                if (Floor != null)
                {
                    errors.Add("floor", "При указании имеющегося заказа этаж должен отсутствовать");
                }
                if (!String.IsNullOrEmpty(PickupPoint))
                {
                    errors.Add("pickup_point", "При указании имеющегося заказа код ПВЗ должен отсутствовать");
                }
            }

            return errors;
        }

    }
}

using DateTimeService.Models.AvailableDeliveryTypes;
using System;
using System.Linq;

namespace DateTimeService.Models
{
    public static class ContractMapping
    {
        public static RequestAvailableDeliveryTypes MapToRequestAvailableDeliveryTypes(this RequestAvailableDeliveryTypesDTO request)
        {
            return new RequestAvailableDeliveryTypes
            {
                CityId = request.CityId,
                PickupPoints = request.PickupPoints,
                OrderItems = request.OrderItems.Select(item => item.MapToRequestAvailableDeliveryTypesItem()).ToList()
            };
        }

        public static RequestAvailableDeliveryTypesItem MapToRequestAvailableDeliveryTypesItem(this RequestAvailableDeliveryTypesItemDTO request)
        {
            return new RequestAvailableDeliveryTypesItem
            {
                Article = request.Code,
                SalesCode = request.SalesCode,
                Quantity = request.Quantity,
                Code = request.SalesCode == null ? null : GetCodeFromSaleCode(request.SalesCode)
            };
        }

        public static RequestDataAvailableDate MapToRequestDataAvailableDate(this RequestDataAvailableDateByCodeItemsDTO request)
        {
            return new RequestDataAvailableDate
            {
                CityId = request.CityId,
                DeliveryTypes = request.DeliveryTypes,
                CheckQuantity = request.CheckQuantity,
                Codes = request.CodeItems.Select(item => item.MapToRequestDataCodeItem()).ToList()
            };
        }

        public static RequestIntervalList MapToRequestIntervalList(this RequestIntervalListDTO request)
        {
            return new RequestIntervalList
            {
                AddressId = request.AddressId,
                DeliveryType = request.DeliveryType,
                PickupPoint = request.PickupPoint,
                Floor = request.Floor,
                Payment = request.Payment,
                OrderNumber = request.OrderNumber,
                OrderDate = request.OrderDate,
                Xcoordinate = request.Xcoordinate,
                Ycoordinate = request.Ycoordinate,
                OrderItems = request.OrderItems.Select(item => item.MapToRequestDataCodeItem()).ToList()
            };
        }

        public static RequestDataCodeItem MapToRequestDataCodeItem(this RequestDataCodeItemDTO request)
        {
            return new RequestDataCodeItem
            {
                Article = request.Code,
                SalesCode = request.SalesCode,
                Quantity = request.Quantity,
                Code = request.SalesCode == null ? null : GetCodeFromSaleCode(request.SalesCode),
                PickupPoints = request.PickupPoints
            };
        }

        //123456 => 00-00123456
        private static string GetCodeFromSaleCode(string saleCode)
        {
            string prefix = "00-";
            int codeLength = 11 - prefix.Length;

            string tempSaleCode = String.Concat("000000000000", saleCode);

            return String.Concat(prefix, tempSaleCode.Substring(tempSaleCode.Length - codeLength, codeLength));
        }
    }
}

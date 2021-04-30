using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace DateTimeService.Models
{
    public class CodeResolver: IValueResolver<RequestOrderItemsDTO, RequestOrderItems, string>
    {
        public string Resolve(RequestOrderItemsDTO source, RequestOrderItems destination, String destMember, ResolutionContext context)
        {
            if (source.sale_code == null)
                return null;
            else
            {
                return GetCodeFromSaleCode(source.sale_code);
            }
        }

        //123456 => 00-00123456
        private string GetCodeFromSaleCode(String saleCode)
        {
            string prefix = "00-";
            int codeLength = 11 - prefix.Length;

            string tempSaleCode = String.Concat("000000000000", saleCode);

            return String.Concat(prefix, tempSaleCode.Substring(tempSaleCode.Length - codeLength, codeLength));
        }
    }
}

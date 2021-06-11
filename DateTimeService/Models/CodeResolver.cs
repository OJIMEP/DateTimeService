using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace DateTimeService.Models
{
    public class CodeResolver: IValueResolver<RequestDataCodeItemDTO, RequestDataCodeItem, string>
    {
        public string Resolve(RequestDataCodeItemDTO source, RequestDataCodeItem destination, String destMember, ResolutionContext context)
        {
            if (source.SalesCode == null)
                return null;
            else
            {
                return GetCodeFromSaleCode(source.SalesCode);
            }
        }

        //123456 => 00-00123456
        private static string GetCodeFromSaleCode(String saleCode)
        {
            string prefix = "00-";
            int codeLength = 11 - prefix.Length;

            string tempSaleCode = String.Concat("000000000000", saleCode);

            return String.Concat(prefix, tempSaleCode.Substring(tempSaleCode.Length - codeLength, codeLength));
        }
    }
}

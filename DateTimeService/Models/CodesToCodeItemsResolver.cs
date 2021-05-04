using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DateTimeService.Models
{
    public class CodesToCodeItemsResolver : IValueResolver<RequestDataAvailableDateByCodesDTO, RequestDataAvailableDate, RequestDataCodeItem[]>
    {
        public RequestDataCodeItem[] Resolve(RequestDataAvailableDateByCodesDTO source, RequestDataAvailableDate destination, RequestDataCodeItem[] destMember, ResolutionContext context)
        {
            var result = new RequestDataCodeItem[source.codes.Length];
            for (int i = 0; i < source.codes.Length; i++)
            {
                result[i] = new RequestDataCodeItem()
                {
                    article = source.codes[i],
                    code = null
                };
            }

            return result;
        }

    }
}

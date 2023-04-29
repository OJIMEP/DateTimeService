using AutoMapper;
using System.Collections.Generic;

namespace DateTimeService.Models
{
    public class CodesToCodeItemsResolver : IValueResolver<RequestDataAvailableDateByCodesDTO, RequestDataAvailableDate, List<RequestDataCodeItem>>
    {
        public List<RequestDataCodeItem> Resolve(RequestDataAvailableDateByCodesDTO source, RequestDataAvailableDate destination, List<RequestDataCodeItem> destMember, ResolutionContext context)
        {
            var result = new List<RequestDataCodeItem>();
            for (int i = 0; i < source.Codes.Length; i++)
            {
                result[i] = new RequestDataCodeItem()
                {
                    Article = source.Codes[i],
                    Code = null
                };
            }

            return result;
        }

    }
}

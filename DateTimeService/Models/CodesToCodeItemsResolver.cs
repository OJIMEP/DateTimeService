using AutoMapper;

namespace DateTimeService.Models
{
    public class CodesToCodeItemsResolver : IValueResolver<RequestDataAvailableDateByCodesDTO, RequestDataAvailableDate, RequestDataCodeItem[]>
    {
        public RequestDataCodeItem[] Resolve(RequestDataAvailableDateByCodesDTO source, RequestDataAvailableDate destination, RequestDataCodeItem[] destMember, ResolutionContext context)
        {
            var result = new RequestDataCodeItem[source.Codes.Length];
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

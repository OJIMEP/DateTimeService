using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace DateTimeService.Models
{
    public class MapperProfile:Profile
    {
        public MapperProfile()
        {
            CreateMap<RequestIntervalListDTO, RequestIntervalList>();
            CreateMap<RequestDataAvailableDateDTO, RequestDataAvailableDate>();

            CreateMap<RequestDataCodeItemDTO, RequestDataCodeItem>()
                .ForMember(dest => dest.code,
                    opt => { opt.MapFrom<CodeResolver>(); })
                .ForMember(dest => dest.article,
                    opt => { opt.MapFrom(src => src.code); });
        }
    }
}

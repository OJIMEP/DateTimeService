﻿using AutoMapper;
using DateTimeService.Models.AvailableDeliveryTypes;

namespace DateTimeService.Models
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<RequestIntervalListDTO, RequestIntervalList>();
            CreateMap<RequestDataAvailableDateByCodeItemsDTO, RequestDataAvailableDate>()
                .ForMember(dest => dest.Codes, opt => opt.MapFrom(src => src.CodeItems));
            CreateMap<RequestDataAvailableDateByCodesDTO, RequestDataAvailableDate>()
                .ForMember(dest => dest.Codes,
                opt => { opt.MapFrom<CodesToCodeItemsResolver>(); });

            CreateMap<RequestDataCodeItemDTO, RequestDataCodeItem>()
                .ForMember(dest => dest.Code,
                    opt => { opt.MapFrom<CodeResolver>(); })
                .ForMember(dest => dest.Article,
                    opt => { opt.MapFrom(src => src.Code); });

            CreateMap<DatabaseInfo, ResponseDatabaseStatusList>();

            CreateMap<RequestAvailableDeliveryTypesDTO, RequestAvailableDeliveryTypes>();

            CreateMap<RequestAvailableDeliveryTypesItemDTO, RequestAvailableDeliveryTypesItem>()
                .ForMember(dest => dest.Code,
                    opt => { opt.MapFrom<AvailableDeliveryTypesCodeResolver>(); })
                .ForMember(dest => dest.Article,
                    opt => { opt.MapFrom(src => src.Code); });
        }
    }
}

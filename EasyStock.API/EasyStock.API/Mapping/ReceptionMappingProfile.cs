﻿using AutoMapper;
using EasyStock.API.Dtos;
using EasyStock.API.Models;

namespace EasyStock.API.Mapping
{
    public class ReceptionMappingProfile : Profile
    {
        public ReceptionMappingProfile()
        {
            CreateMap<Reception, ReceptionOverview>()
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.Name));
            CreateMap<Reception, OutputReceptionDetailDto>();
            CreateMap<ReceptionOverview, OutputReceptionOverviewDto>();
            CreateMap<CreateReceptionDto, Reception>();
            CreateMap<UpdateReceptionDto, Reception>();
        }
    }
}

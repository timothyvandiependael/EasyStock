using AutoMapper;
using EasyStock.API.Dtos;
using EasyStock.API.Models;

namespace EasyStock.API.Mapping
{
    public class DispatchMappingProfile : Profile
    {
        public DispatchMappingProfile()
        {
            CreateMap<Dispatch, DispatchOverview>()
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client.Name));
            CreateMap<Dispatch, OutputDispatchDetailDto>();
            CreateMap<DispatchOverview, OutputDispatchOverviewDto>();
            CreateMap<CreateDispatchDto, Dispatch>();
            CreateMap<UpdateDispatchDto, Dispatch>()
                .ForMember(dest => dest.CrDate, opt => opt.Ignore())
                .ForMember(dest => dest.CrUserId, opt => opt.Ignore());
            CreateMap<Dispatch, Dispatch>();
        }
    }
}

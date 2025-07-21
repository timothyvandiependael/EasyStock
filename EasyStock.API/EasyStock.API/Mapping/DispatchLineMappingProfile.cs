using AutoMapper;
using EasyStock.API.Dtos;
using EasyStock.API.Models;

namespace EasyStock.API.Mapping
{
    public class DispatchLineMappingProfile : Profile
    {
        public DispatchLineMappingProfile()
        {
            CreateMap<DispatchLine, DispatchLineOverview>()
                .ForMember(dest => dest.DispatchNumber, opt => opt.MapFrom(src => src.Dispatch.DispatchNumber))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));
            CreateMap<DispatchLine, OutputDispatchLineDetailDto>();
            CreateMap<DispatchLineOverview, OutputDispatchLineOverviewDto>();
            CreateMap<CreateDispatchLineDto, DispatchLine>();
            CreateMap<UpdateDispatchLineDto, DispatchLine>()
                .ForMember(dest => dest.CrDate, opt => opt.Ignore())
                .ForMember(dest => dest.CrUserId, opt => opt.Ignore());
        }
    }
}

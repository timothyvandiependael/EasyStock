using AutoMapper;
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
            CreateMap<Reception, OutputReceptionOverviewDto>();
            CreateMap<CreateReceptionDto, Reception>();
            CreateMap<UpdateReceptionDto, Reception>()
                .ForMember(dest => dest.CrDate, opt => opt.Ignore())
                .ForMember(dest => dest.CrUserId, opt => opt.Ignore());
            CreateMap<Reception, Reception>();
        }
    }
}

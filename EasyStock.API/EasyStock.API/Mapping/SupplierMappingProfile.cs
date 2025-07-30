using EasyStock.API.Dtos;
using EasyStock.API.Models;
using AutoMapper;

namespace EasyStock.API.Mapping
{
    public class SupplierMappingProfile : Profile
    {
        public SupplierMappingProfile()
        {
            CreateMap<Supplier, OutputSupplierOverviewDto>();
            CreateMap<Supplier, OutputSupplierDetailDto>();
            CreateMap<CreateSupplierDto, Supplier>();
            CreateMap<UpdateSupplierDto, Supplier>()
                .ForMember(dest => dest.CrDate, opt => opt.Ignore())
                .ForMember(dest => dest.CrUserId, opt => opt.Ignore());
            CreateMap<Supplier, Supplier>();
        }
    }
}

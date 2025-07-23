using EasyStock.API.Dtos;
using EasyStock.API.Models;
using AutoMapper;
using EasyStock.API.Common;

namespace EasyStock.API.Mapping
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<Product, ProductOverview>()
                .ForMember(dest => dest.AutoRestockSupplierName, opt => opt.MapFrom(src => src.AutoRestockSupplier == null ? "none" : src.AutoRestockSupplier.Name))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
            CreateMap<Product, OutputProductDetailDto>()
                .ForMember(dest => dest.Photo, opt => opt.MapFrom(src => Base64Helper.ToBase64String(src.Photo)));
            CreateMap<ProductOverview, OutputProductOverviewDto>();
            CreateMap<CreateProductDto, Product>()
                .ForMember(
                    dest => dest.Photo,
                    opt => opt.MapFrom(src => Base64Helper.ToByteArray(src.Photo))
                );
            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.CrDate, opt => opt.Ignore())
                .ForMember(dest => dest.CrUserId, opt => opt.Ignore())
                .ForMember(
                    dest => dest.Photo,
                    opt => opt.MapFrom(src => Base64Helper.ToByteArray(src.Photo))
                );
            CreateMap<Product, Product>();
        }
    }
}

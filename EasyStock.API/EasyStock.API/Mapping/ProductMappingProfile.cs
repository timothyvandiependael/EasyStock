using EasyStock.API.Dtos;
using EasyStock.API.Models;
using AutoMapper;

namespace EasyStock.API.Mapping
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<Product, ProductOverview>()
                .ForMember(dest => dest.AutoRestockSupplierName, opt => opt.MapFrom(src => src.AutoRestockSupplier == null ? "none" : src.AutoRestockSupplier.Name))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
            CreateMap<Product, OutputProductDetailDto>();
            CreateMap<ProductOverview, OutputProductOverviewDto>();
            CreateMap<CreateProductDto, Product>();
            CreateMap<UpdateProductDto, Product>();
        }
    }
}

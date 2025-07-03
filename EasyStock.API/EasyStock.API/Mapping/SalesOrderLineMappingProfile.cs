using AutoMapper;
using EasyStock.API.Dtos;
using EasyStock.API.Models;

namespace EasyStock.API.Mapping
{
    public class SalesOrderLineMappingProfile : Profile
    {
        public SalesOrderLineMappingProfile()
        {
            CreateMap<SalesOrderLine, SalesOrderLineOverview>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));
            CreateMap<SalesOrderLine, OutputSalesOrderLineDetailDto>();
            CreateMap<SalesOrderLineOverview, OutputSalesOrderLineOverviewDto>();
            CreateMap<CreateSalesOrderLineDto, SalesOrderLine>();
            CreateMap<UpdateSalesOrderLineDto, SalesOrderLine>();
        }
    }
}

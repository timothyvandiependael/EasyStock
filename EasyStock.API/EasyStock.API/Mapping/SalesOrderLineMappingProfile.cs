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
                .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.SalesOrder.OrderNumber))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));
            CreateMap<SalesOrderLine, OutputSalesOrderLineDetailDto>();
            CreateMap<SalesOrderLine, OutputSalesOrderLineOverviewDto>();
            CreateMap<SalesOrderLineOverview, OutputSalesOrderLineOverviewDto>();
            CreateMap<CreateSalesOrderLineDto, SalesOrderLine>();
            CreateMap<UpdateSalesOrderLineDto, SalesOrderLine>()
                .ForMember(dest => dest.CrDate, opt => opt.Ignore())
                .ForMember(dest => dest.CrUserId, opt => opt.Ignore());
            CreateMap<SalesOrderLine, SalesOrderLine>();
        }
    }
}

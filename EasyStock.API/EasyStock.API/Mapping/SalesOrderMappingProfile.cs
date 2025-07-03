using AutoMapper;
using EasyStock.API.Dtos;
using EasyStock.API.Models;

namespace EasyStock.API.Mapping
{
    public class SalesOrderMappingProfile : Profile
    {
        public SalesOrderMappingProfile()
        {
            CreateMap<SalesOrder, SalesOrderOverview>()
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client.Name));
            CreateMap<SalesOrder, OutputSalesOrderDetailDto>();
            CreateMap<SalesOrderOverview, OutputSalesOrderOverviewDto>();
            CreateMap<CreateSalesOrderDto, SalesOrder>();
            CreateMap<UpdateSalesOrderDto, SalesOrder>();
        }
    }
}

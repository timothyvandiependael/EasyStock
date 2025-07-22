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
            CreateMap<UpdateSalesOrderDto, SalesOrder>()
                .ForMember(dest => dest.CrDate, opt => opt.Ignore())
                .ForMember(dest => dest.CrUserId, opt => opt.Ignore());
            CreateMap<SalesOrder, SalesOrder>();
        }
    }
}

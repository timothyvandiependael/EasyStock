using AutoMapper;
using EasyStock.API.Dtos;
using EasyStock.API.Models;

namespace EasyStock.API.Mapping
{
    public class PurchaseOrderLineMappingProfile : Profile
    {
        public PurchaseOrderLineMappingProfile()
        {
            CreateMap<PurchaseOrderLine, PurchaseOrderLineOverview>()
                .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.PurchaseOrder.OrderNumber))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));
            CreateMap<PurchaseOrderLine, OutputPurchaseOrderLineDetailDto>();
            CreateMap<PurchaseOrderLineOverview, OutputPurchaseOrderLineOverviewDto>();
            CreateMap<CreatePurchaseOrderLineDto, PurchaseOrderLine>();
            CreateMap<UpdatePurchaseOrderLineDto, PurchaseOrderLine>();
        }
    }
}

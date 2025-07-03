using AutoMapper;
using EasyStock.API.Dtos;
using EasyStock.API.Models;

namespace EasyStock.API.Mapping
{
    public class PurchaseOrderLineLineMappingProfile : Profile
    {
        public PurchaseOrderLineLineMappingProfile()
        {
            CreateMap<PurchaseOrderLine, PurchaseOrderLineOverview>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));
            CreateMap<PurchaseOrderLine, OutputPurchaseOrderLineDetailDto>();
            CreateMap<PurchaseOrderLine, OutputPurchaseOrderLineOverviewDto>();
            CreateMap<CreatePurchaseOrderLineDto, PurchaseOrderLine>();
            CreateMap<UpdatePurchaseOrderLineDto, PurchaseOrderLine>();
        }
    }
}

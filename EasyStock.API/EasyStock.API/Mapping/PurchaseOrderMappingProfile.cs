using AutoMapper;
using EasyStock.API.Dtos;
using EasyStock.API.Models;
namespace EasyStock.API.Mapping
{
    public class PurchaseOrderMappingProfile : Profile
    {
        public PurchaseOrderMappingProfile()
        {
            CreateMap<PurchaseOrder, PurchaseOrderOverview>()
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.Name));
            CreateMap<PurchaseOrder, OutputPurchaseOrderDetailDto>();
            CreateMap<PurchaseOrderOverview, OutputPurchaseOrderOverviewDto>();
            CreateMap<CreatePurchaseOrderDto, PurchaseOrder>();
            CreateMap<UpdatePurchaseOrderDto, PurchaseOrder>();
        }
    }
}

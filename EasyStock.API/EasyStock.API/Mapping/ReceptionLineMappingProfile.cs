using AutoMapper;
using EasyStock.API.Dtos;
using EasyStock.API.Models;

namespace EasyStock.API.Mapping
{
    public class ReceptionLineMappingProfile : Profile
    {
        public ReceptionLineMappingProfile()
        {
            CreateMap<ReceptionLine, ReceptionLineOverview>()
                .ForMember(dest => dest.ReceptionNumber, opt => opt.MapFrom(src => src.Reception.ReceptionNumber))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.PurchaseOrderLink, opt => opt.MapFrom(src => src.PurchaseOrderLine.PurchaseOrder.OrderNumber + "/" + src.PurchaseOrderLine.LineNumber));
            CreateMap<ReceptionLine, OutputReceptionLineDetailDto>();
            CreateMap<ReceptionLine, OutputReceptionLineOverviewDto>();
            CreateMap<ReceptionLineOverview, OutputReceptionLineOverviewDto>();
            CreateMap<CreateReceptionLineDto, ReceptionLine>();
            CreateMap<UpdateReceptionLineDto, ReceptionLine>()
                .ForMember(dest => dest.CrDate, opt => opt.Ignore())
                .ForMember(dest => dest.CrUserId, opt => opt.Ignore());
            CreateMap<ReceptionLine, ReceptionLine>();
        }
    }
}

using AutoMapper;
using EasyStock.API.Dtos;
using EasyStock.API.Models;

namespace EasyStock.API.Mapping
{
    public class StockMovementMappingProfile : Profile
    {
        public StockMovementMappingProfile()
        {
            CreateMap<StockMovement, StockMovementOverview>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));
            CreateMap<StockMovement, OutputStockMovementDetailDto>();
            CreateMap<StockMovementOverview, OutputStockMovementOverviewDto>();
            CreateMap<CreateStockMovementDto, StockMovement>();
            CreateMap<UpdateStockMovementDto, StockMovement>()
                .ForMember(dest => dest.CrDate, opt => opt.Ignore())
                .ForMember(dest => dest.CrUserId, opt => opt.Ignore());
        }
    }
}

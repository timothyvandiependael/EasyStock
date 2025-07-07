namespace EasyStock.API.Dtos
{
    public class OutputStockMovementDetailDto : BaseOutputStockMovementDto
    {
        public required OutputProductOverviewDto Product { get; set; }
    }
}

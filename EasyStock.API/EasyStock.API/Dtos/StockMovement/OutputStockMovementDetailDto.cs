namespace EasyStock.API.Dtos
{
    public class OutputStockMovementDetailDto : BaseOutputStockMovementDto
    {
        public OutputProductOverviewDto Product { get; set; }
    }
}

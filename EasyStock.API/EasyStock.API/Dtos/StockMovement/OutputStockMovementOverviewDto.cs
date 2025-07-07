namespace EasyStock.API.Dtos
{
    public class OutputStockMovementOverviewDto : BaseOutputStockMovementDto
    {
        public required string ProductName { get; set; }
    }
}

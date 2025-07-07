namespace EasyStock.API.Models
{
    public class StockMovementOverview : StockMovement
    {
        public required string ProductName { get; set; }
    }
}

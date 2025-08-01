namespace EasyStock.API.Dtos
{
    public class AutoRestockDto
    {
        public string ProductName { get; set; } = "";
        public bool AutoRestocked { get; set; } = false;
        public string? AutoRestockPurchaseOrderNumber { get; set; }
    }
}

namespace EasyStock.API.Models
{
    public class PurchaseOrderLineOverview : PurchaseOrderLine
    {
        public required string OrderNumber { get; set; }
        public required string ProductName { get; set; }
    }
}

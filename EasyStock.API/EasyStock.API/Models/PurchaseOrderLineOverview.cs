namespace EasyStock.API.Models
{
    public class PurchaseOrderLineOverview : PurchaseOrderLine
    {
        public string OrderNumber { get; set; }
        public string ProductName { get; set; }
    }
}

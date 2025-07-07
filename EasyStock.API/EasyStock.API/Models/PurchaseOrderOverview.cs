namespace EasyStock.API.Models
{
    public class PurchaseOrderOverview : PurchaseOrder
    {
        public required string SupplierName { get; set; }
    }
}

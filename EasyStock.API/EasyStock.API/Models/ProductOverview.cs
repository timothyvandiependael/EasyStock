namespace EasyStock.API.Models
{
    public class ProductOverview : Product
    {
        public required string AutoRestockSupplierName { get; set; }
        public required string CategoryName { get; set; }
    }
}

namespace EasyStock.API.Models
{
    public class ProductOverview : Product
    {
        public string AutoRestockSupplierName { get; set; }
        public string CategoryName { get; set; }
    }
}

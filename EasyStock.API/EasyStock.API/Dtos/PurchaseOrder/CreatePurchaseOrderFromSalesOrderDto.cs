namespace EasyStock.API.Dtos
{
    public class CreatePurchaseOrderFromSalesOrderDto
    {
        public int SalesOrderId { get; set; }
        public Dictionary<int, int> ProductSuppliers { get; set; } = new Dictionary<int, int>();
    }
}

namespace EasyStock.API.Models
{
    public class SalesOrderLineOverview : SalesOrderLine
    {
        public required string OrderNumber { get; set; }
        public required string ProductName { get; set; }
    }
}

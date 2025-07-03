namespace EasyStock.API.Models
{
    public class SalesOrderLineOverview : SalesOrderLine
    {
        public string OrderNumber { get; set; }
        public string ProductName { get; set; }
    }
}

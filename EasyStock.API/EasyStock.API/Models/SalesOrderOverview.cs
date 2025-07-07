namespace EasyStock.API.Models
{
    public class SalesOrderOverview : SalesOrder
    {
        public required string ClientName { get; set; }
    }
}

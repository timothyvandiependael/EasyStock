namespace EasyStock.API.Dtos
{
    public class OutputSalesOrderLineOverviewDto : BaseOutputSalesOrderLineDto
    {
        public required string OrderNumber { get; set; }
        public required string ProductName { get; set; }
    }
}

namespace EasyStock.API.Dtos
{
    public class OutputSalesOrderLineOverviewDto : BaseOutputSalesOrderLineDto
    {
        public string OrderNumber { get; set; }
        public string ProductName { get; set; }
    }
}

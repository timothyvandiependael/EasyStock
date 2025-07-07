namespace EasyStock.API.Dtos
{
    public class OutputSalesOrderLineDetailDto : BaseOutputSalesOrderLineDto
    {
        public required OutputSalesOrderOverviewDto SalesOrder { get; set; }
        public required OutputProductOverviewDto Product { get; set; }
    }
}

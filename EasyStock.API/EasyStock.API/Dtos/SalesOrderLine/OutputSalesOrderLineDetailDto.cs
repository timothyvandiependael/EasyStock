namespace EasyStock.API.Dtos
{
    public class OutputSalesOrderLineDetailDto : BaseOutputSalesOrderLineDto
    {
        public OutputSalesOrderOverviewDto SalesOrder { get; set; }
        public OutputProductOverviewDto Product { get; set; }
    }
}

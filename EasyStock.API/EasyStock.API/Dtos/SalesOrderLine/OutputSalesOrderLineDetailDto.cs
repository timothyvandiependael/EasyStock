namespace EasyStock.API.Dtos
{
    public class OutputSalesOrderLineDetailDto : BaseOutputSalesOrderLineDto
    {
        public required OutputSalesOrderOverviewDto SalesOrder { get; set; }
        public required OutputProductOverviewDto Product { get; set; }
        public List<OutputDispatchLineOverviewDto>? DispatchLines { get; set; }
        public int DispatchedQuantity { get; set; }
    }
}

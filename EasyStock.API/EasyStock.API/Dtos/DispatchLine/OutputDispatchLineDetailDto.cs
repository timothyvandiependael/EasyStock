namespace EasyStock.API.Dtos
{
    public class OutputDispatchLineDetailDto : BaseOutputDispatchLineDto
    {
        public required OutputDispatchOverviewDto Dispatch { get; set; }
        public required OutputProductOverviewDto Product { get; set; }
        public required OutputSalesOrderLineOverviewDto SalesOrderLine { get; set; }
    }
}

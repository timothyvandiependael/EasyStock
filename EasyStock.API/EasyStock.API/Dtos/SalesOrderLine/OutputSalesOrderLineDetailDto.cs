namespace EasyStock.API.Dtos
{
    public class OutputSalesOrderLineDetailDto : BaseOutputSalesOrderLineDto
    {
        public required OutputSalesOrderOverviewDto SalesOrder { get; set; }
        public required OutputProductOverviewDto Product { get; set; }
        public List<OutputDispatchLineOverviewDto>? DispatchLines { get; set; }
        public int DispatchedQuantity { get; set; }

        public bool DecreasedStockBelowMinimum { get; set; } = false;
        public bool AutoRestocked { get; set; } = false;
        public int AutoRestockPurchaseOrderId { get; set; }
        public string? AutoRestockPurchaseOrderNumber { get; set; }
    }
}

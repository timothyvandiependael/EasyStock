namespace EasyStock.API.Dtos
{
    public class OutputPurchaseOrderLineOverviewDto : BaseOutputPurchaseOrderLineDto
    {
        public required string OrderNumber { get; set; }
        public required string ProductName { get; set; }
        public int DeliveredQuantity { get; set; }
    }
}

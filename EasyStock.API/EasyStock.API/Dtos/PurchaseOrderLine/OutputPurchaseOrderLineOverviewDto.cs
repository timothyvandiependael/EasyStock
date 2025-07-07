namespace EasyStock.API.Dtos
{
    public class OutputPurchaseOrderLineOverviewDto : BaseOutputPurchaseOrderLineDto
    {
        public string OrderNumber { get; set; }
        public string ProductName { get; set; }
        public int DeliveredQuantity { get; set; }
    }
}

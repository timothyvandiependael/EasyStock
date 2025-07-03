namespace EasyStock.API.Dtos
{
    public class OutputPurchaseOrderLineDetailDto : BaseOutputPurchaseOrderLineDto
    {
        public OutputPurchaseOrderOverviewDto PurchaseOrder { get; set; }
        public OutputProductOverviewDto Product { get; set; }
    }
}

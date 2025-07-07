namespace EasyStock.API.Dtos
{
    public class OutputPurchaseOrderOverviewDto : BaseOutputPurchaseOrderDto
    {
        public required string SupplierName { get; set; }
    }
}

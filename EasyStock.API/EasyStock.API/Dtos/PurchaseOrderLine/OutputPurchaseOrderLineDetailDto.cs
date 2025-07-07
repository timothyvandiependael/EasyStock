using EasyStock.API.Models;

namespace EasyStock.API.Dtos
{
    public class OutputPurchaseOrderLineDetailDto : BaseOutputPurchaseOrderLineDto
    {
        public required OutputPurchaseOrderOverviewDto PurchaseOrder { get; set; }
        public required OutputProductOverviewDto Product { get; set; }
        public List<OutputReceptionLineOverviewDto>? ReceptionLines { get; set; }
        public int DeliveredQuantity { get; set; }
    }
}

using EasyStock.API.Models;

namespace EasyStock.API.Dtos
{
    public class OutputPurchaseOrderLineDetailDto : BaseOutputPurchaseOrderLineDto
    {
        public OutputPurchaseOrderOverviewDto PurchaseOrder { get; set; }
        public OutputProductOverviewDto Product { get; set; }
        public List<OutputReceptionLineOverviewDto>? ReceptionLines { get; set; }
        public int DeliveredQuantity { get; set; }
    }
}

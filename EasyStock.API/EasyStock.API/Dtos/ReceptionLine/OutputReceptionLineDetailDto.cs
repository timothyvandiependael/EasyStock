using EasyStock.API.Models;
using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class OutputReceptionLineDetailDto : BaseOutputReceptionLineDto
    {
        public required OutputReceptionOverviewDto Reception { get; set; }
        public required OutputProductOverviewDto Product { get; set; }
        public required OutputPurchaseOrderLineOverviewDto PurchaseOrderLine { get; set; }
    }
}

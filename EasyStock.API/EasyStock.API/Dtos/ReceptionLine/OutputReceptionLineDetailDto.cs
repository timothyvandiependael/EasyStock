using EasyStock.API.Models;
using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class OutputReceptionLineDetailDto : BaseOutputReceptionLineDto
    {
        public OutputReceptionOverviewDto Reception { get; set; }
        public OutputProductOverviewDto Product { get; set; }
        public OutputPurchaseOrderLineOverviewDto PurchaseOrderLine { get; set; }
    }
}

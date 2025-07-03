using EasyStock.API.Dtos;
using EasyStock.API.Models;

namespace EasyStock.API.Dtos
{
    public class OutputPurchaseOrderDetailDto : BaseOutputPurchaseOrderDto
    {
        public OutputSupplierDto Supplier { get; set; }

        public List<OutputPurchaseOrderLineOverviewDto> Lines { get; set; } = new List<OutputPurchaseOrderLineOverviewDto>();
    }
}

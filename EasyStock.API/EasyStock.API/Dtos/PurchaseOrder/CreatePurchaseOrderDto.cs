using EasyStock.API.Models;

namespace EasyStock.API.Dtos
{
    public class CreatePurchaseOrderDto
    {
        public string? Comments { get; set; }
        public int SupplierId { get; set; }

        public List<CreatePurchaseOrderLineDto> Lines { get; set; } = new List<CreatePurchaseOrderLineDto>();
    }
}

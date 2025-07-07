using EasyStock.API.Models;

namespace EasyStock.API.Dtos
{
    public class BaseOutputPurchaseOrderDto : OutputDtoBase
    {
        public int Id { get; set; }
        public required string OrderNumber { get; set; }

        public string? Comments { get; set; }

        public int SupplierId { get; set; }
        public required string Status { get; set; }

    }
}

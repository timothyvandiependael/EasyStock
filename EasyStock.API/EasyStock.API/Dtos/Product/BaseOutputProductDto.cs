using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class BaseOutputProductDto : OutputDtoBase
    {
        public int Id { get; set; }
        public string SKU { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public byte[]? Photo { get; set; }
        public decimal CostPrice { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal Discount { get; set; }
        public int TotalStock { get; set; }
        public int ReservedStock { get; set; }
        public int InboundStock { get; set; }
        public int AvailableStock { get; set; }
        public int MinimumStock { get; set; }
        public bool AutoRestock { get; set; }
        public int? AutoRestockSupplierId { get; set; }
        public int CategoryId { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Models
{
    public class Product : ModelBase, IEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string SKU { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public byte[]? Photo { get; set; }


        [Range(0, double.MaxValue)]
        public decimal CostPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal RetailPrice { get; set; }

        [Range(0, 100)]
        public decimal Discount { get; set; }

        [Range(0, int.MaxValue)]
        public int TotalStock { get; set; }

        [Range(0, int.MaxValue)]
        public int ReservedStock { get; set; }

        [Range(0, int.MaxValue)]
        public int InboundStock { get; set; }

        [Range(0, int.MaxValue)]
        public int AvailableStock { get; set; }

        [Range(0, int.MaxValue)]
        public int MinimumStock { get; set; }

        public bool AutoRestock { get; set; }

        public int? AutoRestockSupplierId { get; set; }

        public Supplier? AutoRestockSupplier { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public required Category Category { get; set; }

    }
}

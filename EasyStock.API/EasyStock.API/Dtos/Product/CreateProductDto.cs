using EasyStock.API.Models;
using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class CreateProductDto
    {
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
        public int MinimumStock { get; set; }

        public bool AutoRestock { get; set; }

        public int? AutoRestockSuppliedId { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}

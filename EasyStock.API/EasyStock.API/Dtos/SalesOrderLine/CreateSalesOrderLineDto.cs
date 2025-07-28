using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class CreateSalesOrderLineDto
    {
        public int SalesOrderId { get; set; }

        [MaxLength(1000)]
        public string? Comments { get; set; }

        public int ProductId { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }
}

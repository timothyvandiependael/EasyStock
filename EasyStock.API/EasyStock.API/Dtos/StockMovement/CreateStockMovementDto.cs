using EasyStock.API.Models;
using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class CreateStockMovementDto
    {
        public int ProductId { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int QuantityChange { get; set; }

        [Required]
        public string Reason { get; set; }

        public int? PurchaseOrderId { get; set; }
        public int? SalesOrderId { get; set; }
    }
}

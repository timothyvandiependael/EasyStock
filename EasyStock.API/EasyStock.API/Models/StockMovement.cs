using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Models
{
    public class StockMovement : ModelBase, IEntity
    {
        public int Id { get; set; }
        [Required]
        public int ProductId { get; set; }
        public required virtual Product Product { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int QuantityChange { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Reason { get; set; }

        public int? PurchaseOrderId { get; set; }
        public int? SalesOrderId { get; set; }
    }
}

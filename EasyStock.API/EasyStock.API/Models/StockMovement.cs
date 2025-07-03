using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Models
{
    public class StockMovement : ModelBase, IEntity
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int QuantityChange { get; set; }

        [Required]
        public string Reason { get; set; }

        public int? PurchaseOrderId { get; set; }
        public int? SalesOrderId { get; set; }
    }
}

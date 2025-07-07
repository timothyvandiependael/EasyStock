using EasyStock.API.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyStock.API.Models
{
    public class PurchaseOrderLine : ModelBase, IEntity
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public PurchaseOrder PurchaseOrder { get; set; }
        public int LineNumber { get; set; }

        [MaxLength(1000)]
        public string? Comments { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "varchar(10)")]
        public OrderStatus Status { get; set; }

        public ICollection<ReceptionLine>? ReceptionLines { get; set; }
        public int DeliveredQuantity =>
            ReceptionLines?.Sum(rl => rl.Quantity) ?? 0;
    }
}

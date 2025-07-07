using EasyStock.API.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyStock.API.Models
{
    public class PurchaseOrder : ModelBase, IEntity
    {
        public int Id { get; set; }
        [Required]
        public required string OrderNumber { get; set; }

        [MaxLength(1000)]
        public string? Comments { get; set; }

        public int SupplierId { get; set; }
        public required Supplier Supplier { get; set; }

        [Column(TypeName = "varchar(10)")]
        public OrderStatus Status { get; set; }

        public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
    }
}

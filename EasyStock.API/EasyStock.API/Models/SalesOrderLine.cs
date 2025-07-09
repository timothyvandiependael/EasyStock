using EasyStock.API.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyStock.API.Models
{
    public class SalesOrderLine : ModelBase, IEntity
    {
        public int Id { get; set; }
        [Required]
        public int SalesOrderId { get; set; }
        public required SalesOrder SalesOrder { get; set; }
        public int LineNumber { get; set; }
        [MaxLength(1000)]
        public string? Comments { get; set; }
        [Required]
        public int ProductId { get; set; }
        public required Product Product { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "varchar(10)")]
        public OrderStatus Status { get; set; }
        public ICollection<DispatchLine>? DispatchLines { get; set; }
        public int DispatchedQuantity =>
            DispatchLines?.Sum(rl => rl.Quantity) ?? 0;
    }
}

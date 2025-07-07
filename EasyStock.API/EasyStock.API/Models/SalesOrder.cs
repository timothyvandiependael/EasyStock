using EasyStock.API.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyStock.API.Models
{
    public class SalesOrder : ModelBase, IEntity
    {
        public int Id { get; set; }
        [Required]
        public required string OrderNumber { get; set; }

        [MaxLength(1000)]
        public string? Comments { get; set; }

        [Required]
        public int ClientId { get; set; }
        public required Client Client { get; set; }

        [Column(TypeName = "varchar(10)")]
        public OrderStatus Status { get; set; }

        public ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();
    }
}

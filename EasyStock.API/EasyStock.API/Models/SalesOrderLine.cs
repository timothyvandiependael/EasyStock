using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Models
{
    public class SalesOrderLine : ModelBase, IEntity
    {
        public int Id { get; set; }
        public int SalesOrderId { get; set; }
        public SalesOrder SalesOrder { get; set; }
        public int LineNumber { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }
}

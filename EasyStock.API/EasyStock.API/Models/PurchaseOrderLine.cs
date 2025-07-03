using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Models
{
    public class PurchaseOrderLine : ModelBase, IEntity
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public PurchaseOrder PurchaseOrder { get; set; }
        public int LineNumber { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }
        public string Status { get; set; }
    }
}

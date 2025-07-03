using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Models
{
    public class PurchaseOrder : ModelBase, IEntity
    {
        public int Id { get; set; }

        public string? Comments { get; set; }

        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }

        public string Status { get; set; }

        public ICollection<PurchaseOrderLine> Lines { get; set; }
    }
}

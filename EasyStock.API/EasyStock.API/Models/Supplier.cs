using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Models
{
    public class Supplier : Person, IEntity
    {
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }
}

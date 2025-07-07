using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Models
{
    public class Supplier : Person, IEntity
    {
        public ICollection<Product> Products { get; set; } = new List<Product>();

        public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }
}

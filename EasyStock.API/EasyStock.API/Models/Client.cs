namespace EasyStock.API.Models
{
    public class Client : Person, IEntity
    {
        public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    }
}

namespace EasyStock.API.Models
{
    public class Client : Person, IEntity
    {
        public ICollection<SalesOrder> SalesOrders { get; set; }
    }
}

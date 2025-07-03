namespace EasyStock.API.Models
{
    public class SalesOrder : ModelBase, IEntity
    {
        public int Id { get; set; }

        public string? Comments { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; }
        public string Status { get; set; }

        public ICollection<SalesOrderLine> Lines { get; set; }
    }
}

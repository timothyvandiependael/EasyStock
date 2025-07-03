namespace EasyStock.API.Models
{
    public class DispatchLine : ModelBase
    {
        public int Id { get; set; }
        public int DispatchId { get; set; }
        public Dispatch Dispatch { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
    }
}

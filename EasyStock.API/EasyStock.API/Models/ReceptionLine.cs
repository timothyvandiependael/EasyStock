namespace EasyStock.API.Models
{
    public class ReceptionLine : ModelBase
    {
        public int Id { get; set; }
        public int ReceptionId { get; set; }
        public Reception Reception { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
    }
}

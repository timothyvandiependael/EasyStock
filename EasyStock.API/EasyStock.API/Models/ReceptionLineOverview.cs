namespace EasyStock.API.Models
{
    public class ReceptionLineOverview : ReceptionLine
    {
        public required string ReceptionNumber { get; set; }
        public required string ProductName { get; set; }
        public required string PurchaseOrderLink { get; set; }
    }
}

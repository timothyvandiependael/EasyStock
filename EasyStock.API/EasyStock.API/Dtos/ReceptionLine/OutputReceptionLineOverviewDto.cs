namespace EasyStock.API.Dtos
{
    public class OutputReceptionLineOverviewDto : BaseOutputReceptionLineDto
    {
        public required string ReceptionNumber { get; set; }
        public required string PurchaseOrderLink { get; set; }
        public required string ProductName { get; set; }

    }
}

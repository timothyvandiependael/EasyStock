namespace EasyStock.API.Dtos
{
    public class OutputSupplierDetailDto : OutputSupplierOverviewDto
    {
        public ICollection<OutputPurchaseOrderOverviewDto> PurchaseOrders { get; set; } 
            = new List<OutputPurchaseOrderOverviewDto>();

        public ICollection<OutputProductOverviewDto> Products { get; set; } = new List<OutputProductOverviewDto>();
    }
}

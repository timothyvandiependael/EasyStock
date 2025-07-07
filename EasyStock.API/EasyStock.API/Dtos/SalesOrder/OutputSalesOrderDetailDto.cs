namespace EasyStock.API.Dtos
{
    public class OutputSalesOrderDetailDto : BaseOutputSalesOrderDto
    {
        public required OutputClientDto Client { get; set; }

        public List<OutputSalesOrderLineOverviewDto> Lines { get; set; } = new List<OutputSalesOrderLineOverviewDto>();
    }
}

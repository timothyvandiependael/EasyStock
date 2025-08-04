namespace EasyStock.API.Dtos
{
    public class OutputSalesOrderDetailDto : BaseOutputSalesOrderDto
    {
        public required OutputClientOverviewDto Client { get; set; }

        public List<OutputSalesOrderLineOverviewDto> Lines { get; set; } = new List<OutputSalesOrderLineOverviewDto>();

        public List<AutoRestockDto> AutoRestockDtos { get; set; } = new List<AutoRestockDto>();
    }
}

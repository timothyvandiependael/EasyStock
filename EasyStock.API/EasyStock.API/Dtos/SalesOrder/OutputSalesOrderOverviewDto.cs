namespace EasyStock.API.Dtos
{
    public class OutputSalesOrderOverviewDto : BaseOutputSalesOrderDto
    {
        public required string ClientName { get; set; }
    }
}

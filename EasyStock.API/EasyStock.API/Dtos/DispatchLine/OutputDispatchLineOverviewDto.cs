namespace EasyStock.API.Dtos
{
    public class OutputDispatchLineOverviewDto : BaseOutputDispatchLineDto
    {
        public required string DispatchNumber { get; set; }
        public required string ProductName { get; set; }
    }
}

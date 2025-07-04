namespace EasyStock.API.Dtos
{
    public class OutputDispatchLineOverviewDto : BaseOutputDispatchLineDto
    {
        public string DispatchNumber { get; set; }
        public string ProductName { get; set; }
    }
}

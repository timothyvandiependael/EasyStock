namespace EasyStock.API.Dtos
{
    public class OutputDispatchLineDetailDto : BaseOutputDispatchLineDto
    {
        public OutputDispatchOverviewDto Dispatch { get; set; }
        public OutputProductOverviewDto Product { get; set; }
    }
}

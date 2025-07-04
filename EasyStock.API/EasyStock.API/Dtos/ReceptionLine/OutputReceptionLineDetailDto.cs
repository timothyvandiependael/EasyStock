namespace EasyStock.API.Dtos
{
    public class OutputReceptionLineDetailDto : BaseOutputReceptionLineDto
    {
        public OutputReceptionOverviewDto Reception { get; set; }
        public OutputProductOverviewDto Product { get; set; }
    }
}

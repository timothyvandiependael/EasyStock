using EasyStock.API.Models;

namespace EasyStock.API.Dtos
{
    public class OutputDispatchDetailDto : BaseOutputDispatchDto
    {
        public required OutputClientDto Client { get; set; }
        public ICollection<OutputDispatchLineOverviewDto> Lines { get; set; } = new List<OutputDispatchLineOverviewDto>();
    }
}

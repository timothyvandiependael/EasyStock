using EasyStock.API.Models;

namespace EasyStock.API.Dtos
{
    public class OutputReceptionDetailDto : BaseOutputReceptionDto
    {
        public required OutputSupplierOverviewDto Supplier { get; set; }
        public ICollection<OutputReceptionLineOverviewDto> Lines { get; set; } = new List<OutputReceptionLineOverviewDto>();
    }
}

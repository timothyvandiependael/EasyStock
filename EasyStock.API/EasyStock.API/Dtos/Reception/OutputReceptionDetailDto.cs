using EasyStock.API.Models;

namespace EasyStock.API.Dtos
{
    public class OutputReceptionDetailDto : BaseOutputReceptionDto
    {
        public required OutputSupplierDto Supplier { get; set; }
        public ICollection<OutputReceptionLineOverviewDto> Lines { get; set; } = new List<OutputReceptionLineOverviewDto>();
    }
}

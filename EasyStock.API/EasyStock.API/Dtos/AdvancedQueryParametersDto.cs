using EasyStock.API.Common;

namespace EasyStock.API.Dtos
{
    public class AdvancedQueryParametersDto
    {
        public List<FilterCondition>? Filters { get; set; }
        public List<SortOption>? Sorting { get; set; }
        public Pagination Pagination { get; set; } = new Pagination();
    }
}

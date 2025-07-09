using EasyStock.API.Dtos;
using EasyStock.API.Models;
using Microsoft.AspNetCore.Connections;

namespace EasyStock.API.Dtos
{
    public class OutputProductDetailDto : BaseOutputProductDto
    {
        public OutputSupplierOverviewDto? AutoRestockSupplier { get; set; }
        public required OutputCategoryDto Category { get; set; }
        public ICollection<OutputSupplierOverviewDto> Suppliers { get; set; } = new List<OutputSupplierOverviewDto>();

    }
}

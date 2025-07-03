using EasyStock.API.Dtos;
using EasyStock.API.Models;

namespace EasyStock.API.Dtos
{
    public class OutputProductDetailDto : BaseOutputProductDto
    {
        public OutputSupplierDto AutoRestockSupplier { get; set; }
        public OutputCategoryDto Category { get; set; }

    }
}

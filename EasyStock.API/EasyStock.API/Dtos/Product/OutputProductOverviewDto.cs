using EasyStock.API.Dtos;
using EasyStock.API.Models;
using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class OutputProductOverviewDto : BaseOutputProductDto
    {
        public required string CategoryName { get; set; }
        public string? SupplierName { get; set; }
    }
}

using EasyStock.API.Models;
using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class CreateDispatchLineDto
    {
        public int DispatchId { get; set; }
        [MaxLength(1000)]
        public string? Comments { get; set; }
        public int ProductId { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
        [Required]
        public int SalesOrderLineId { get; set; }
    }
}

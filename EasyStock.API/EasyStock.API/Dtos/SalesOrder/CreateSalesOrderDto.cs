using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class CreateSalesOrderDto
    {
        [MaxLength(1000)]
        public string? Comments { get; set; }
        public int ClientId { get; set; }
        public List<CreateSalesOrderLineDto> Lines { get; set; } = new List<CreateSalesOrderLineDto>();
    }
}

using EasyStock.API.Models;

namespace EasyStock.API.Dtos
{
    public class BaseOutputSalesOrderDto : OutputDtoBase
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string? Comments { get; set; }
        public int ClientId { get; set; }
        public string Status { get; set; }
    }
}

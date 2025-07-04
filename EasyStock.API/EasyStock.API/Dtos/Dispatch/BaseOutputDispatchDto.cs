using EasyStock.API.Models;

namespace EasyStock.API.Dtos
{
    public class BaseOutputDispatchDto : OutputDtoBase
    {
        public int Id { get; set; }
        public string DispatchNumber { get; set; }
        public int ClientId { get; set; }
    }
}

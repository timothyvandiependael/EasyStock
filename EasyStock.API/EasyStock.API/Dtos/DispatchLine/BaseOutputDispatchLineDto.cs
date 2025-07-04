using EasyStock.API.Models;

namespace EasyStock.API.Dtos
{
    public class BaseOutputDispatchLineDto : OutputDtoBase
    {
        public int Id { get; set; }
        public int DispatchId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}

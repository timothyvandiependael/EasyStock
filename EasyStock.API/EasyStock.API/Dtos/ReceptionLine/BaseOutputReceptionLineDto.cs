using EasyStock.API.Models;

namespace EasyStock.API.Dtos
{
    public class BaseOutputReceptionLineDto : OutputDtoBase
    {
        public int Id { get; set; }
        public int ReceptionId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}

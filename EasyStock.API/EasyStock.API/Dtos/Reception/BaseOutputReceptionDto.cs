using EasyStock.API.Models;

namespace EasyStock.API.Dtos
{
    public class BaseOutputReceptionDto : OutputDtoBase
    {
        public int Id { get; set; }
        public string ReceptionNumber { get; set; }
        public int SupplierId { get; set; }
    }
}

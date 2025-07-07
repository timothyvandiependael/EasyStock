using EasyStock.API.Models;
using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class BaseOutputReceptionDto : OutputDtoBase
    {
        public int Id { get; set; }
        public required string ReceptionNumber { get; set; }
        public string? Comments { get; set; }
        public int SupplierId { get; set; }
    }
}

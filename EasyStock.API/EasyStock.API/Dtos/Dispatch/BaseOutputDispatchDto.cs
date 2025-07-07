using EasyStock.API.Models;
using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class BaseOutputDispatchDto : OutputDtoBase
    {
        public int Id { get; set; }
        public string? Comments { get; set; }
        public required string DispatchNumber { get; set; }
        public int ClientId { get; set; }
    }
}

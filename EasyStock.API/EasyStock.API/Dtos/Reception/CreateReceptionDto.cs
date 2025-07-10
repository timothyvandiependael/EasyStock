using EasyStock.API.Models;
using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class CreateReceptionDto
    {
        [MaxLength(1000)]
        public string? Comments { get; set; }
        public int SupplierId { get; set; }

        public List<CreateReceptionLineDto> Lines { get; set; } = new List<CreateReceptionLineDto>();

    }
}

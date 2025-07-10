using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class CreateDispatchDto
    {
        [MaxLength(1000)]
        public string? Comments { get; set; }
        public int ClientId { get; set; }
        public List<CreateDispatchLineDto> Lines { get; set; } = new List<CreateDispatchLineDto>();
    }
}

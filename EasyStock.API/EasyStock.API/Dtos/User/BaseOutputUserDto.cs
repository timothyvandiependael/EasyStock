using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class BaseOutputUserDto : OutputDtoBase
    {
        public int Id { get; set; }

        [Required]
        public required string UserName { get; set; }
    }
}

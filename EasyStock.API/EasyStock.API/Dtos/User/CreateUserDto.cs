using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class CreateUserDto
    {
        [Required]
        [MaxLength(20)]
        public required string UserName { get; set; }
    }
}

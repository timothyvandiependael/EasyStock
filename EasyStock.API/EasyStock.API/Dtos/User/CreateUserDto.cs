using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class CreateUserDto
    {
        [Required]
        public string UserName { get; set; }
    }
}

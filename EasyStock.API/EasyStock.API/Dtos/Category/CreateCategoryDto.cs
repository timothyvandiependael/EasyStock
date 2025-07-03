using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class CreateCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }
}

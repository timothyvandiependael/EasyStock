using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class UpdateCategoryDto : CreateCategoryDto
    {
        public int Id { get; set; }
    }
}

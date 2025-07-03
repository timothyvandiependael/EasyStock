using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Models
{
    public class Category : ModelBase, IEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
    }
}

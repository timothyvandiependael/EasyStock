using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Models
{
    public class User : ModelBase, IEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public required string UserName { get; set; }

        public ICollection<UserPermission> Permissions { get; set; } = new List<UserPermission>();
    }
}

using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Models
{
    public class UserAuth : ModelBase, IEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public required string UserName { get; set; }

        [Required]
        public required string PasswordHash { get; set; }

        public required string Role { get; set; }
        public bool MustChangePassword { get; set; } = true;
    }
}

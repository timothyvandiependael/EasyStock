using EasyStock.API.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Column(TypeName = "varchar(10)")]
        public UserRole Role { get; set; }
        public bool MustChangePassword { get; set; } = true;
    }
}

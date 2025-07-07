using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Models
{
    public class UserAuth : ModelBase, IEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string UserName { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string Role { get; set; }
    }
}

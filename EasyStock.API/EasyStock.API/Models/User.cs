using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Models
{
    public class User : ModelBase, IEntity
    {
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; }

        public ICollection<UserPermission> Permissions { get; set; }
    }
}

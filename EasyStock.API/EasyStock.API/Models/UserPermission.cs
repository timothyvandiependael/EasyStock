using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Models
{
    public class UserPermission : ModelBase, IEntity
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public required virtual User User { get; set; }

        [Required]
        [MaxLength(30)]
        public required string Resource { get; set; } 

        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}

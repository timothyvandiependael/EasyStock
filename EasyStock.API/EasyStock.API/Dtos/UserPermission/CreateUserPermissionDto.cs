using EasyStock.API.Models;

namespace EasyStock.API.Dtos
{
    public class CreateUserPermissionDto
    {
        public int UserId { get; set; }
        public required string Resource { get; set; }
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}

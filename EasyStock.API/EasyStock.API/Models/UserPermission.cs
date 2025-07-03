namespace EasyStock.API.Models
{
    public class UserPermission : ModelBase
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public string Resource { get; set; } 
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}

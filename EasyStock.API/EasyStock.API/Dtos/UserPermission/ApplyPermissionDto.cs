namespace EasyStock.API.Dtos
{
    public class ApplyPermissionDto
    {
        public required string Resource { get; set; }
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}

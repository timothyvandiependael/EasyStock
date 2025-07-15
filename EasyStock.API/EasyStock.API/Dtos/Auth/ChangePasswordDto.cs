namespace EasyStock.API.Dtos
{
    public class ChangePasswordDto
    {
        public required string UserName { get; set; }
        public string? OldPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}

namespace EasyStock.API.Dtos
{
    public class ChangePasswordDto
    {
        public int UserId { get; set; }
        public required string OldPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}

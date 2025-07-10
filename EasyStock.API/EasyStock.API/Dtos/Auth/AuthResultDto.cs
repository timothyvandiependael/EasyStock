namespace EasyStock.API.Dtos.Auth
{
    public class AuthResultDto
    {
        public required string Token { get; set; }
        public bool MustChangePassword { get; set; }
    }
}

namespace EasyStock.API.Dtos
{
    public class ChangePasswordResultDto
    {
        public bool Success { get; set; }
        public Dictionary<string, string> Errors { get; set; } = new Dictionary<string, string>();
    }
}

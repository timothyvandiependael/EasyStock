namespace EasyStock.API.Common
{
    public class ValidationRules
    {
        public bool Required { get; set; } = false;
        public int MaxLength { get; set; } = 0;
        public int MinLength { get; set; } = 0;
        public int Min { get; set; } = 0;
        public int Max { get; set; } = 0;
        public bool IsPassword { get; set; } = false;
        public bool IsEmail { get; set; } = false;
        public bool IsUrl { get; set; } = false;
        public string Pattern { get; set; } = string.Empty;
    }
}

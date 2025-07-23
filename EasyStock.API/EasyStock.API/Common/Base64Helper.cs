namespace EasyStock.API.Common
{
    public static class Base64Helper
    {
        public static byte[]? ToByteArray(string? base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
                return null;

            // If the string has a data URL prefix, strip it
            var clean = base64.Contains(",")
                ? base64.Substring(base64.IndexOf(',') + 1)
                : base64;

            return Convert.FromBase64String(clean);
        }

        public static string? ToBase64String(byte[]? data)
        {
            return data != null ? $"data:image/png;base64,{Convert.ToBase64String(data)}" : null;
        }
    }
}

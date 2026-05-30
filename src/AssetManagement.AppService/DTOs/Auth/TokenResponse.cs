namespace AssetManagement.AppService.DTOs
{
    public class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public string ExpiryIn { get; set; } = string.Empty;
    }
}

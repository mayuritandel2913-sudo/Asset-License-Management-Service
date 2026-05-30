using System.Text.Json.Serialization;

namespace AssetManagement.AppService.DTOs.License;

public class GetLicenseRequest
{
    [JsonPropertyName("licenseId")]
    public int LicenseId { get; set; }
    [JsonPropertyName("licenseName")]
    public string LicenseName { get; set; } = string.Empty;
    [JsonPropertyName("vendor")]
    public string? Vendor { get; set; }
    [JsonPropertyName("purchaseDate")]
    public string? PurchaseDate { get; set; }
    [JsonPropertyName("expiryDate")]
    public string? ExpiryDate { get; set; }
    [JsonPropertyName("statusId")]
    public byte StatusId { get; set; }
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
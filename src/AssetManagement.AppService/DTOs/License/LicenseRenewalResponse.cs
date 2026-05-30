using System.Text.Json.Serialization;

namespace AssetManagement.AppService.DTOs.License;

public class LicenseRenewalResponse
{
    [JsonPropertyName("renewalId")]
    public int RenewalId { get; set; }

    [JsonPropertyName("licenseId")]
    public int LicenseId { get; set; }

    [JsonPropertyName("renewalDate")]
    public string RenewalDate { get; set; } = null!;

    [JsonPropertyName("expiryDate")]
    public string ExpiryDate { get; set; } = null!;

    [JsonPropertyName("updatedTotalSeats")]
    public int? UpdatedTotalSeats { get; set; }

    [JsonPropertyName("updatedCost")]
    public decimal? UpdatedCost { get; set; }

    [JsonPropertyName("licenseKey")]
    public string? LicenseKey { get; set; }

    [JsonPropertyName("renewalNotes")]
    public string? RenewalNotes { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AssetManagement.AppService.Validation;
using AssetManagement.Utility.Resource;

namespace AssetManagement.AppService.DTOs.License;

[RenewalDatesValidation]
public class RenewLicenseRequest
{
    [JsonPropertyName("licenseId")]
    public int LicenseId { get; set; }

    [JsonPropertyName("renewalDate")]
    [Required(ErrorMessage = "Please enter renewal date.")]
    [RegularExpression(@"^(0[1-9]|1[0-2])/(0[1-9]|[12]\d|3[01])/\d{4}$", ErrorMessage = CommonResource.PurchaseDate_Format)]
    public string? RenewalDate { get; set; }

    [JsonPropertyName("expiryDate")]
    [Required(ErrorMessage = CommonResource.ExpiryDate_Format)]
    [RegularExpression(@"^(0[1-9]|1[0-2])/(0[1-9]|[12]\d|3[01])/\d{4}$", ErrorMessage = CommonResource.ExpiryDate_Format)]
    public string? ExpiryDate { get; set; }

    [JsonPropertyName("updatedTotalSeats")]
    [Range(1, byte.MaxValue, ErrorMessage = CommonResource.TotalSeats_Invalid)]
    public int? UpdatedTotalSeats { get; set; }

    [JsonPropertyName("updatedCost")]
    [Range(typeof(decimal), "0.01", "999999999.99", ErrorMessage = "Updated cost must be greater than zero.")]
    public decimal? UpdatedCost { get; set; }

    [JsonPropertyName("updatedLicenseKey")]
    [RegularExpression(@"^[a-zA-Z0-9._-]*$", ErrorMessage = CommonResource.LicenseKey_Format)]
    public string? UpdatedLicenseKey { get; set; }

    [JsonPropertyName("renewalNotes")]
    [MaxLength(500, ErrorMessage = CommonResource.Description_MaxLength)]
    public string? RenewalNotes { get; set; }
}


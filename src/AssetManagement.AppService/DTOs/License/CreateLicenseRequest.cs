using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AssetManagement.Utility.Resource;
using AssetManagement.AppService.Validation;

namespace AssetManagement.AppService.DTOs.License;

[LicenseDatesValidation]
public class CreateLicenseRequest
{
    [JsonPropertyName("name")]
    [Required(ErrorMessage = CommonResource.LicenseName_Required)]
    [MinLength(2, ErrorMessage = CommonResource.LicenseName_MinLength)]
    [MaxLength(100, ErrorMessage = CommonResource.LicenseName_MaxLength)]
    [RegularExpression(@"^[a-zA-Z0-9 ]+$", ErrorMessage = "License name can only contain letters, digits.")]
    public string LicenseName { get; set; } = null!;

    [JsonPropertyName("licenseTypeId")]
    [Required(ErrorMessage = CommonResource.LicenseType_Required)]
    [Range(1, byte.MaxValue, ErrorMessage = CommonResource.LicenseType_Invalid)]
    public byte LicenseTypeID { get; set; }

    [JsonPropertyName("licensePurchaseTypeId")]
    [Required(ErrorMessage = CommonResource.LicensePurchaseType_Required)]
    [Range(1, byte.MaxValue, ErrorMessage = CommonResource.LicensePurchaseType_Invalid)]
    public byte LicensePurchaseTypeID { get; set; }

    [JsonPropertyName("totalSeats")]
    [Required(ErrorMessage = CommonResource.TotalSeats_Required)]
    [Range(1, byte.MaxValue, ErrorMessage = CommonResource.TotalSeats_Invalid)]
    public double? TotalSeats { get; set; }

    [JsonPropertyName("vendor")]
    [MinLength(2, ErrorMessage = CommonResource.VendorName_MinLength)]
    [MaxLength(50, ErrorMessage = CommonResource.VendorName_MaxLength)]
    public string? VendorName { get; set; }

    [JsonPropertyName("purchaseDate")]
    [Required(ErrorMessage = CommonResource.PurchaseDate_Required)]
    [RegularExpression(@"^(0[1-9]|1[0-2])/(0[1-9]|[12]\d|3[01])/\d{4}$", ErrorMessage = CommonResource.PurchaseDate_Format)]
    public string? PurchaseDate { get; set; }

    [JsonPropertyName("startDate")]
    [Required(ErrorMessage = CommonResource.StartDate_Required)]
    [RegularExpression(@"^(0[1-9]|1[0-2])/(0[1-9]|[12]\d|3[01])/\d{4}$", ErrorMessage = CommonResource.StartDate_Format)]
    public string? StartDate { get; set; }

    [JsonPropertyName("expiryDate")]
    [RegularExpression(@"^(0[1-9]|1[0-2])/(0[1-9]|[12]\d|3[01])/\d{4}$", ErrorMessage = CommonResource.ExpiryDate_Format)]
    public string? ExpiryDate { get; set; }

    [JsonPropertyName("licenseKey")]
    [RegularExpression(@"^[a-zA-Z0-9._-]*$", ErrorMessage = CommonResource.LicenseKey_Format)]
    public string? LicenseKey { get; set; }

    [JsonPropertyName("cost")]
    [Required(ErrorMessage = CommonResource.Cost_Required)]
    [Range(typeof(decimal), "-999999999.99", "999999999.99")]
    public decimal? Cost { get; set; }

    [JsonPropertyName("reminderDays")]
    public List<ReminderConfigRequest>? ReminderDays { get; set; }

    [JsonPropertyName("description")]
    [MaxLength(500, ErrorMessage = CommonResource.Description_MaxLength)]
    public string? Description { get; set; }

}

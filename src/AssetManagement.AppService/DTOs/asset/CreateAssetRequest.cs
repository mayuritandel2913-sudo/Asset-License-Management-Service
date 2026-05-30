using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using AssetManagement.Utility.Resource;

namespace AssetManagement.AppService.DTOs;

[System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class StrictDateAttribute : ValidationAttribute
{
    public StrictDateAttribute() : base("please enter the purchaseDate in MM/DD/YYYY formate")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return true;

        string[] formats = { "MM-dd-yyyy", "MM/dd/yyyy" };
        return DateTime.TryParseExact(value.ToString(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }
}

public class CreateAssetRequest: IValidatableObject
{
    [JsonPropertyName("assetName")]
    [Required(ErrorMessage = "name is required")]
    [MinLength(2, ErrorMessage = "Minimum 2 characters are required.")]
    [MaxLength(50, ErrorMessage = "Maximum 50 characters are required.")]
    [RegularExpression(@"^[a-zA-Z0-9 ]+$", ErrorMessage = "Asset name can only contain letters, digits.")]
    public string AssetName { get; set; } = null!;

    [JsonPropertyName("serialNumber")]
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string SerialNumber { get; set; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "Please select asset category.")]
    public int CategoryID { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "statusId must be greater than 0.")]
    [JsonPropertyName("statusId")]
    public int StatusID { get; set; } = 1;

    [JsonPropertyName("purchaseDate")]
    [Required(ErrorMessage = "Please enter the purchase date.")]
    [StrictDate]
    public string? PurchaseDateString { get; set; }

    [JsonIgnore]
    public DateTime? PurchaseDate
    {
        get
        {
            if (string.IsNullOrWhiteSpace(PurchaseDateString)) return null;
            string[] formats = { "MM-dd-yyyy", "MM/dd/yyyy" };
            if (DateTime.TryParseExact(PurchaseDateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }
            return null;
        }
        set
        {
            PurchaseDateString = value?.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
        }
    }

    [JsonPropertyName("assetCost")]
    [Required(ErrorMessage = "Asset cost is required.")]
    [Range(typeof(decimal), "-999999999.99", "999999999.99")]
    public decimal? AssetCost { get; set; }

    [JsonPropertyName("vendorName")]
    [MinLength(2, ErrorMessage = CommonResource.VendorNameMinLength)]
    [MaxLength(50, ErrorMessage = CommonResource.VendorNameMaxLength)]
    public string? VendorName { get; set; }

    [JsonPropertyName("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [JsonPropertyName("assetProperties")]
    public List<CreateAssetRequestProperty>? AssetProperties { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AssetCost.HasValue)
        {
            if (AssetCost == 0)
            {
                yield return new ValidationResult("Asset cost must be greater than zero.", new[] { nameof(AssetCost) });
            }
            else if (AssetCost < 0)
            {
                yield return new ValidationResult("Asset cost must be positive value.", new[] { nameof(AssetCost) });
            }
        }

        if (PurchaseDate.HasValue && PurchaseDate.Value.Date > DateTime.UtcNow.Date)
        {
            yield return new ValidationResult("Purchase date cannot be in the future.", new[] { nameof(PurchaseDateString) });
        }
    }
}

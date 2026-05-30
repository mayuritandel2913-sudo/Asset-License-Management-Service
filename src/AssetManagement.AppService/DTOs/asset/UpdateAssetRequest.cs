using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json.Serialization;

namespace AssetManagement.AppService.DTOs;

public class UpdateAssetRequest : IValidatableObject
{

    [JsonPropertyName("assetName")]
    [Required(ErrorMessage = "Asset name is required.")]
    [MinLength(2, ErrorMessage = "Minimum 2 characters are required.")]
    [MaxLength(50, ErrorMessage = "Maximum 50 characters are required.")]
    [RegularExpression(@"^[a-zA-Z0-9 ]+$", ErrorMessage = "Asset name can only contain letters, digits.")]
    public string? AssetName { get; set; }

    [Required(ErrorMessage = "Serial number is required.")]
    public string? SerialNumber { get; set; }

    [Required(ErrorMessage = "Category id is required.")]
    public int? CategoryID { get; set; }

    public int? StatusID { get; set; }

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

    public string VendorName { get; set; } = null!;

    public string? Description { get; set; }

    [JsonPropertyName("assetProperties")]
    public List<CreateAssetRequestProperty>? AssetProperties { get; set; }
    
    [JsonPropertyName("assetHealthStatusId")]
    [Required(ErrorMessage = "Select Asset Health Status.")]
    [Range(1, int.MaxValue, ErrorMessage = "Asset health status value should be greater than zero.")]
    public int? AssetHealthStatusID { get; set; }

    [JsonPropertyName("healthRemark")]
    [MaxLength(100, ErrorMessage = "Health remark cannot exceed 100 characters.")]
    public string? HealthRemark { get; set; }
    

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

// Change: create custom validation annotation (instead of using IValidatableObject inline in RenewLicenseRequest).
// Review comment by: Dimpal Patel (Maintainer)

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using AssetManagement.Utility.Resource;

namespace AssetManagement.AppService.Validation;

// #region RenewalDatesValidationAttribute - Custom Validation Annotation
/// <summary>
/// Custom class-level validation attribute for RenewLicenseRequest.
/// Validates that RenewalDate and ExpiryDate are in the correct format,
/// and that ExpiryDate is strictly after RenewalDate.
/// Replaces the inline IValidatableObject.Validate() implementation.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class RenewalDatesValidationAttribute : ValidationAttribute
{
    private const string DateFormat = "MM/dd/yyyy";
    private const string RenewalDateProp = "RenewalDate";
    private const string ExpiryDateProp = "ExpiryDate";
    private const string UpdatedCostProp = "UpdatedCost";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null) return ValidationResult.Success;

        var renewalDateStr = value.GetType().GetProperty(RenewalDateProp)?.GetValue(value) as string;
        var expiryDateStr = value.GetType().GetProperty(ExpiryDateProp)?.GetValue(value) as string;
        var updatedCost = value.GetType().GetProperty(UpdatedCostProp)?.GetValue(value) as decimal?;

        if (!TryParseDate(renewalDateStr, out var renewalDate))
        {
            return new ValidationResult(CommonResource.PurchaseDate_Format, new[] { RenewalDateProp });
        }

        if (!TryParseDate(expiryDateStr, out var expiryDate))
        {
            return new ValidationResult(CommonResource.ExpiryDate_Format, new[] { ExpiryDateProp });
        }

        if (expiryDate.Date <= renewalDate.Date)
        {
            return new ValidationResult("Expiry date must be after renewal date.", new[] { ExpiryDateProp });
        }

        if (updatedCost.HasValue && updatedCost.Value <= 0)
        {
            return new ValidationResult("Updated cost must be greater than zero.", new[] { UpdatedCostProp });
        }

        return ValidationResult.Success;
    }

    private static bool TryParseDate(string? value, out DateTime parsedDate) =>
        DateTime.TryParseExact(
            value,
            DateFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out parsedDate);
}
// #endregion RenewalDatesValidationAttribute - Custom Validation Annotation

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using AssetManagement.Utility.Resource;

namespace AssetManagement.AppService.Validation;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class LicenseDatesValidationAttribute : ValidationAttribute
{
    private const string DateFormat = "MM/dd/yyyy";
    private const string PurchaseProp = "PurchaseDate";
    private const string StartProp = "StartDate";
    private const string ExpiryProp = "ExpiryDate";
    private const string TotalSeatsProp = "TotalSeats";
    private const string CostProp = "Cost";

    public bool AllowPastExpiryDate { get; set; }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null) return ValidationResult.Success;

        if (!TryReadDate(value, PurchaseProp, out var purchaseDateParsed, out var purchaseError))
        {
            return purchaseError;
        }

        if (!TryReadDate(value, StartProp, out var startDateParsed, out var startError))
        {
            return startError;
        }

        if (purchaseDateParsed.Date > DateTime.UtcNow.Date)
        {
            return new ValidationResult(CommonResource.PurchaseDate_FutureDate, new[] { PurchaseProp });
        }

        var twoYearsAgo = DateTime.UtcNow.AddYears(-2);
        if (purchaseDateParsed.Date < twoYearsAgo.Date)
        {
            return new ValidationResult(CommonResource.PurchaseDate_TooOld, new[] { PurchaseProp });
        }

        if (startDateParsed.Date < purchaseDateParsed.Date)
        {
            return new ValidationResult(CommonResource.StartDate_BeforePurchase, new[] { StartProp });
        }

        if (!ValidateTotalSeats(value, out var totalSeatsError))
        {
            return totalSeatsError;
        }

        if (!ValidateCost(value, out var costError))
        {
            return costError;
        }

        return ValidateExpiryDate(value, purchaseDateParsed, startDateParsed);
    }

    private static bool ValidateTotalSeats(object source, out ValidationResult? validationResult)
    {
        var totalSeatsProperty = source.GetType().GetProperty(TotalSeatsProp);
        var totalSeatsValue = totalSeatsProperty?.GetValue(source);
        if (totalSeatsValue is null)
        {
            validationResult = null;
            return true;
        }

        var totalSeats = Convert.ToDouble(totalSeatsValue, CultureInfo.InvariantCulture);
        if (Math.Abs(totalSeats - Math.Truncate(totalSeats)) > double.Epsilon)
        {
            validationResult = new ValidationResult("Only integer value is allowed for TotalSeats.", new[] { TotalSeatsProp });
            return false;
        }

        validationResult = null;
        return true;
    }

    private static bool ValidateCost(object source, out ValidationResult? validationResult)
    {
        var costProperty = source.GetType().GetProperty(CostProp);
        var costValue = costProperty?.GetValue(source);
        if (costValue is null)
        {
            validationResult = null;
            return true;
        }

        var cost = Convert.ToDecimal(costValue, CultureInfo.InvariantCulture);
        if (cost == 0)
        {
            validationResult = new ValidationResult("Cost must be greater than zero.", new[] { CostProp });
            return false;
        }

        if (cost < 0)
        {
            validationResult = new ValidationResult("Cost must be positive value.", new[] { CostProp });
            return false;
        }

        validationResult = null;
        return true;
    }

    private ValidationResult? ValidateExpiryDate(object source, DateTime purchaseDate, DateTime startDate)
    {
        var expiryValue = source.GetType().GetProperty(ExpiryProp)?.GetValue(source) as string;
        if (string.IsNullOrWhiteSpace(expiryValue))
        {
            return ValidationResult.Success;
        }

        if (!TryParseDate(expiryValue, out var expiryDateParsed))
        {
            return new ValidationResult(CommonResource.ExpiryDate_Format, new[] { ExpiryProp });
        }

        if (expiryDateParsed.Date <= purchaseDate.Date)
        {
            return new ValidationResult(CommonResource.ExpiryDate_BeforePurchase, new[] { ExpiryProp });
        }

        if (expiryDateParsed.Date <= startDate.Date)
        {
            return new ValidationResult(CommonResource.ExpiryDate_BeforeStartDate, new[] { ExpiryProp });
        }

        if (!AllowPastExpiryDate && expiryDateParsed.Date <= DateTime.UtcNow.Date)
        {
            return new ValidationResult(CommonResource.ExpiryDate_PastDate, new[] { ExpiryProp });
        }

        return ValidationResult.Success;
    }

    private static bool TryReadDate(object source, string propertyName, out DateTime date, out ValidationResult? validationResult)
    {
        var value = source.GetType().GetProperty(propertyName)?.GetValue(source) as string;
        if (TryParseDate(value, out date))
        {
            validationResult = null;
            return true;
        }

        validationResult = new ValidationResult(
            propertyName == PurchaseProp ? CommonResource.PurchaseDate_Format : CommonResource.StartDate_Format,
            new[] { propertyName });
        return false;
    }

    private static bool TryParseDate(string? value, out DateTime parsedDate)
    {
        return DateTime.TryParseExact(
            value,
            DateFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out parsedDate);
    }
}

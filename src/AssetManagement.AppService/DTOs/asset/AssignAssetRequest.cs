using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json.Serialization;
using AssetManagement.AppService.Validators;

namespace AssetManagement.AppService.DTOs;

public class AssignAssetRequest : IValidatableObject
{
    public int UserID { get; set; }

    [JsonPropertyName("assignmentDate")]
    [DateStringToDate(nameof(AssignmentDate), "MM/dd/yyyy", required: true)]
    public string? AssignmentDateString { get; set; }

    [JsonIgnore]
    public DateTime? AssignmentDate { get; set; }

    [JsonPropertyName("expectedReturnDate")]
    [DateStringToDate(nameof(ExpectedReturnDate), "MM/dd/yyyy", required: false)]
    public string? ExpectedReturnDateString { get; set; }

    [JsonIgnore]
    public DateTime? ExpectedReturnDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (UserID == 0)
        {
            yield return new ValidationResult(
                "User ID is required.",
                new[] { nameof(UserID) });
        }
        else if (UserID < 0)
        {
            yield return new ValidationResult(
                "UserID must be greater than 0.",
                new[] { nameof(UserID) });
        }

        
        if (ExpectedReturnDate.HasValue && AssignmentDate.HasValue && ExpectedReturnDate.Value < AssignmentDate.Value)
        {
            yield return new ValidationResult(
                "Date is invalid.",
                new[] { nameof(ExpectedReturnDateString) });
        }
    }
}

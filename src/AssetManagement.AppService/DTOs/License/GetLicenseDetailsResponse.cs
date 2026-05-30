using System.Text.Json.Serialization;

namespace AssetManagement.AppService.DTOs.License;

public class GetLicenseDetailsResponse
{
    [JsonPropertyName("licenseId")]
    public int LicenseId { get; set; }
    
    [JsonPropertyName("licenseName")]
    public string LicenseName { get; set; } = string.Empty;
    
    [JsonPropertyName("vendor")]
    public string? Vendor { get; set; }
    
    [JsonPropertyName("licenseTypeId")]
    public byte LicenseTypeId { get; set; }
    
    [JsonPropertyName("licenseType")]
    public string LicenseType { get; set; } = string.Empty;
    
    [JsonPropertyName("licensePurchaseTypeId")]
    public byte LicensePurchaseTypeId { get; set; }
    
    [JsonPropertyName("licensePurchaseType")]
    public string LicensePurchaseType { get; set; } = string.Empty;
    
    [JsonPropertyName("maskedlicenseKey")]
    public string? MaskedLicenseKey { get; set; }
    
    [JsonPropertyName("unmaskedlicenseKey")]
    public string? UnmaskedLicenseKey { get; set; }
    
    [JsonPropertyName("purchaseDate")]
    public string? PurchaseDate { get; set; }
    
    [JsonPropertyName("startDate")]
    public string? StartDate { get; set; }
    
    [JsonPropertyName("expiryDate")]
    public string? ExpiryDate { get; set; }
    
    [JsonPropertyName("cost")]
    public decimal Cost { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("statusId")]
    public byte StatusId { get; set; }
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("seatInfo")]
    public SeatInfoDto SeatInfo { get; set; } = new SeatInfoDto();
    
    [JsonPropertyName("assignmentDetails")]
    public List<AssignmentDetailsDto> AssignmentDetails { get; set; } = new List<AssignmentDetailsDto>();
}

public class SeatInfoDto
{
    [JsonPropertyName("totalSeats")]
    public int TotalSeats { get; set; }
    
    [JsonPropertyName("assignedSeats")]
    public int AssignedSeats { get; set; }
    
    [JsonPropertyName("availableSeats")]
    public int AvailableSeats { get; set; }
}

public class AssignmentDetailsDto
{
    [JsonPropertyName("assignedTo")]
    public string AssignedTo { get; set; } = string.Empty;
    
    [JsonPropertyName("assignmentDate")]
    public string AssignmentDate { get; set; } = string.Empty;
    
    [JsonPropertyName("department")]
    public string? Department { get; set; }
    
    [JsonPropertyName("AssignBy")]
    public string AssignBy { get; set; } = string.Empty;
}
using System.Text.Json.Serialization;

namespace AssetManagement.AppService.DTOs.License;

public class LicenseAssignmentResponse
{
    [JsonPropertyName("licenseAssignmentID")]
    public int LicenseAssignmentID { get; set; }

    [JsonPropertyName("assignedTo")]
    public string AssignedTo { get; set; } = null!;

    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;

    [JsonPropertyName("assignmentDate")]
    public DateTime AssignmentDate { get; set; }

    [JsonPropertyName("department")]
    public string? Department { get; set; }

    [JsonPropertyName("assignedBy")]
    public string AssignedBy { get; set; } = null!;
}

public class PaginatedLicenseAssignmentResponse
{
    [JsonPropertyName("assignments")]
    public List<LicenseAssignmentResponse> Assignments { get; set; } = new();

    [JsonPropertyName("totalRecords")]
    public int TotalRecords { get; set; }

    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages => (TotalRecords + PageSize - 1) / PageSize;

    [JsonPropertyName("hasAssignments")]
    public bool HasAssignments => TotalRecords > 0;
}

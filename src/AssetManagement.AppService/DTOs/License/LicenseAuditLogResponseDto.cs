using System.Text.Json.Serialization;

namespace AssetManagement.AppService.DTOs.License;

public class LicenseAuditLogItem
{
    [JsonPropertyName("logId")]
    public int LogId { get; set; }
    [JsonPropertyName("dateTime")]
    public string DateTime { get; set; } = string.Empty;
    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;
    [JsonPropertyName("performedBy")]
    public string PerformedBy { get; set; } = string.Empty;
    [JsonPropertyName("employeeName")]
    public string? EmployeeName { get; set; }
    [JsonPropertyName("employeeEmail")]
    public string? EmployeeEmail { get; set; }
}

public class CustomPagination
{
    [JsonPropertyName("page")]
    public int Page { get; set; }
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }
    [JsonPropertyName("totalRecords")]
    public int TotalRecords { get; set; }
    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }
}

public class LicenseAuditLogData
{
    [JsonPropertyName("licenseId")]
    public int LicenseId { get; set; }
    [JsonPropertyName("licenseName")]
    public string LicenseName { get; set; } = string.Empty;
    [JsonPropertyName("licenseLogs")]
    public List<LicenseAuditLogItem> LicenseLogs { get; set; } = new();
    [JsonPropertyName("pagination")]
    public CustomPagination Pagination { get; set; } = new();
}

public class LicenseAuditLogPagedResponse
{
    [JsonPropertyName("data")]
    public List<LicenseAuditLogData> Data { get; set; } = new();
}
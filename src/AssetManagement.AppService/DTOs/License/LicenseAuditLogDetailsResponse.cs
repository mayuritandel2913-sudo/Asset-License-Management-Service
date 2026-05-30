using System.Text.Json.Serialization;

namespace AssetManagement.AppService.DTOs.License;

public class UpdatedDetail
{
    [JsonPropertyName("fieldName")]
    public string FieldName { get; set; } = string.Empty;

    [JsonPropertyName("oldValue")]
    public string? OldValue { get; set; }

    [JsonPropertyName("newValue")]
    public string? NewValue { get; set; }
}

public class LicenseAuditLogDetailData
{
    [JsonPropertyName("logId")]
    public int LogId { get; set; }
    [JsonPropertyName("licenseId")]
    public int LicenseId { get; set; }
    [JsonPropertyName("licenseName")]
    public string LicenseName { get; set; } = string.Empty;
    [JsonPropertyName("updatedDetails")]
    public List<UpdatedDetail> UpdatedDetails { get; set; } = new();
}

public class LicenseAuditLogDetailsResponse
{
    [JsonPropertyName("data")]
    public List<LicenseAuditLogDetailData>? Data { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }
}
using System.Text.Json.Serialization;

namespace AssetManagement.AppService.DTOs.License;

public class AuditLogRequest
{
    [JsonPropertyName("pageNo")]
    public int PageNo { get; set; } = 1;
}
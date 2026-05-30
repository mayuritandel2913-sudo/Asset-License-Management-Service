using System.Text.Json.Serialization;

namespace AssetManagement.AppService.DTOs;

public class AssetAuditLogDetailResponse
{
    [JsonPropertyName("fieldName")]
    public string FieldName { get; set; } = string.Empty;

    [JsonPropertyName("oldValue")]
    public string? OldValue { get; set; }

    [JsonPropertyName("newValue")]
    public string? NewValue { get; set; }
}

public class AssetAuditLogResponse
{
    [JsonPropertyName("logId")]
    public int LogId { get; set; } 

    [JsonPropertyName("dateTime")]
    public string DateTime { get; set; } = string.Empty;

    [JsonPropertyName("actionPerformed")]
    public string ActionPerformed { get; set; } = string.Empty;

    [JsonPropertyName("performedBy")]
    public string PerformedBy { get; set; } = string.Empty;

    [JsonPropertyName("employeeName")]
    public string? EmployeeName { get; set; }

    [JsonPropertyName("employeeEmail")]
    public string? EmployeeEmail { get; set; }
}

public class AssetAuditLogDetailItemResponse
{
    [JsonPropertyName("logId")]
    public int LogId { get; set; } 

    [JsonPropertyName("assetId")]
    public int AssetId { get; set; }

    [JsonPropertyName("assetName")]
    public string AssetName { get; set; } = string.Empty;

    [JsonPropertyName("updatedDetails")]
    public List<AssetAuditLogDetailResponse> UpdatedDetails { get; set; } = new();
}

public class AssetAuditLogDetailPagedResponse
{
    [JsonPropertyName("data")]
    public List<AssetAuditLogDetailItemResponse>? Data { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }
}

public class AssetAuditLogPagedResponse
{
    [JsonPropertyName("data")]
    public List<AssetAuditLogGroupResponse> Data { get; set; } = new();
}

public class AssetAuditLogGroupResponse
{
    [JsonPropertyName("assetId")]
    public int AssetId { get; set; }

    [JsonPropertyName("assetName")]
    public string AssetName { get; set; } = string.Empty;

    [JsonPropertyName("assetLogs")]
    public List<AssetAuditLogResponse> AssetLogs { get; set; } = new();

    [JsonPropertyName("pagination")]
    public AssetAuditLogPaginationResponse Pagination { get; set; } = new();
}

public class AssetAuditLogPaginationResponse
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalRecords")]
    public int TotalRecords { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }
}

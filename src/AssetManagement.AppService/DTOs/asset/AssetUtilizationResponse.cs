using System.Text.Json.Serialization;

namespace AssetManagement.AppService.DTOs;

public class AssetUtilizationCategoryResponse
{
    [JsonPropertyName("categoryName")]
    public string CategoryName { get; set; } = string.Empty;

    [JsonPropertyName("assignedAsset")]
    public int AssignedAsset { get; set; }

    [JsonPropertyName("unassignedAsset")]
    public int UnassignedAsset { get; set; }
}

public class AssetUtilizationChartResponse
{
    [JsonPropertyName("categories")]
    public List<AssetUtilizationCategoryResponse> Categories { get; set; } = new();
}

public class AssetUtilizationPagedResponse
{
    [JsonPropertyName("data")]
    public List<AssetUtilizationChartResponse>? Data { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }
}


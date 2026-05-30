using System.Text.Json.Serialization;

namespace AssetManagement.AppService.DTOs;

public class AssetByCategoryResponse
{
    [JsonPropertyName("categoryName")]
    public string CategoryName { get; set; } = string.Empty;

    [JsonPropertyName("assetCount")]
    public int AssetCount { get; set; }

    [JsonPropertyName("percentage")]
    public decimal Percentage { get; set; }
}

public class AssetByCategoryChartResponse
{
    [JsonPropertyName("categories")]
    public List<AssetByCategoryResponse> Categories { get; set; } = new();
}

public class AssetByCategoryPagedResponse
{
    [JsonPropertyName("data")]
    public List<AssetByCategoryChartResponse>? Data { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }
}
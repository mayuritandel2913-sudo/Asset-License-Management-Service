using System.Text.Json.Serialization;

namespace AssetManagement.AppService.DTOs;

public class AssetDowntimeCategoryResponse
{
    [JsonPropertyName("categoryName")]
    public string CategoryName { get; set; } = string.Empty;

    [JsonPropertyName("downtimePercentage")]
    public decimal DowntimePercentage { get; set; }
}

public class AssetDowntimeChartResponse
{
    [JsonPropertyName("categories")]
    public List<AssetDowntimeCategoryResponse> Categories { get; set; } = new();
}

public class AssetDowntimePagedResponse
{
    [JsonPropertyName("data")]
    public List<AssetDowntimeChartResponse>? Data { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }
}
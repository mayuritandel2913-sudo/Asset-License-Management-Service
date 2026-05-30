using System.Text.Json.Serialization;

namespace AssetManagement.AppService.DTOs;

public class AssetResponse
{
    [JsonPropertyName("assetId")]
    public int AssetID { get; set; }

    [JsonPropertyName("assetName")]
    public string AssetName { get; set; } = null!;

    [JsonPropertyName("categoryName")]
    public string CategoryName { get; set; } = null!;

    [JsonPropertyName("serialNumber")]
    public string SerialNumber { get; set; } = null!;

    [JsonPropertyName("purchaseDate")]
    public string? PurchaseDateFormatted { get; set; }

    [JsonPropertyName("assetCost")]
    public decimal? AssetCost { get; set; }

    [JsonPropertyName("vendorName")]
    public string VendorName { get; set; } = null!;

    [JsonPropertyName("assetStatus")]
    public string? AssetStatus { get; set; }

    [JsonPropertyName("assetHealthStatusName")]
    public string? AssetHealthStatusName { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = null!;

    [JsonPropertyName("assignedEmployee")]
    public string? AssignedEmployee { get; set; }

    [JsonPropertyName("resourceTypeName")]
    public string ResourceTypeName { get; set; } = null!;

    [JsonPropertyName("properties")]
    public List<ResponseAssetProperty> Properties { get; set; } = new();
}

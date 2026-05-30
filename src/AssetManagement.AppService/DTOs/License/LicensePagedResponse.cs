using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AssetManagement.AppService.DTOs.License;

public class LicensePagedResponse
{
    [JsonPropertyName("ListOfLicense")]
    public IEnumerable<GetLicenseRequest> Data { get; set; } = new List<GetLicenseRequest>();
    [JsonPropertyName("pageNo")]
    public int PageNo { get; set; }
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }
    [JsonPropertyName("totalRecords")]
    public int TotalRecords { get; set; }
    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
    [JsonPropertyName("search")]
    public string? Search { get; set; }
    [JsonPropertyName("filter")]
    public string? Filter { get; set; }
}
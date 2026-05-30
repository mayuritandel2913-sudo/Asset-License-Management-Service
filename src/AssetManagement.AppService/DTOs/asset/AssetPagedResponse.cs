namespace AssetManagement.AppService.DTOs;

public class AssetPagedResponse
{
    public List<AssetResponse> Assets { get; set; } = new();
    public int PageNo { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public string? Search { get; set; }
}
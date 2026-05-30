namespace AssetManagement.AppService.DTOs;

public class AssetReportFilterRequest
{
    public string? Status { get; set; }
    public string? Category { get; set; }
    public string? HealthStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNo { get; set; } = 1;
}


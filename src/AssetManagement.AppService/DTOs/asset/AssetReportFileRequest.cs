namespace AssetManagement.AppService.DTOs;

public class AssetReportFileRequest
{
    public string? Status { get; set; }
    public string? Category { get; set; }
    public string? HealthStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Format { get; set; } = "excel";
    public string? Fields { get; set; }
}


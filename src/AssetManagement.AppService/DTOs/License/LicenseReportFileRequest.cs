
namespace AssetManagement.AppService.DTOs.License;

public class LicenseReportFileRequest
{
    public string? Filter { get; set; }
    public string? Search { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Format { get; set; } = "excel";
    public string? Fields { get; set; }
}


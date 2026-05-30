namespace AssetManagement.Utility.Report;

public class LicenseReportItem
{
    public int LicenseId { get; set; }
    public string LicenseName { get; set; } = string.Empty;
    public string LicenseType { get; set; } = string.Empty;
    public string PurchaseType { get; set; } = string.Empty;
    public byte TotalSeats { get; set; }
  
    public int AvailableSeats { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string LicenseStatus { get; set; } = string.Empty;
    public string LicenseKey { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string Description { get; set; } = string.Empty;
   
    public string AssignedEmployees { get; set; } = string.Empty;
    public DateTime? AssignmentDate { get; set; }
    public string AssignedBy { get; set; } = string.Empty;
    public DateTime? UnassignedDate { get; set; }
    public string UnassignedBy { get; set; } = string.Empty;
}

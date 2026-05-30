namespace AssetManagement.AppService.DTOs.License;

public class LicenseResponse
{
    public int LicenseID { get; set; }
    public string LicenseName { get; set; } = string.Empty;
    public byte LicenseTypeID { get; set; }
    public byte LicensePurchaseTypeID { get; set; }
    public byte TotalSeats { get; set; }
    public string? VendorName { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public byte LicenseStatusID { get; set; }
    public string? LicenseKey { get; set; }
    public decimal Cost { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
}
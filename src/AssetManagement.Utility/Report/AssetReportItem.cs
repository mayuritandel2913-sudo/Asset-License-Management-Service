using System;

namespace AssetManagement.Utility.Report
{
    public class AssetReportItem
    {
        public int AssetId { get; set; }
        public string? AssetName { get; set; }
        public string? CategoryName { get; set; }
        public string? SerialNumber { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal AssetCost { get; set; }
        public string? VendorName { get; set; }
        public string? AssetStatus { get; set; }
        public string? AssignmentStatus { get; set; }
        public string? HealthStatus { get; set; }
        public string? Description { get; set; }
        public int? AssignmentID { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime? AssignmentDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public string? PropertyName { get; set; }
    }
}

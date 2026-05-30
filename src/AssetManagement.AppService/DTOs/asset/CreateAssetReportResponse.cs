namespace AssetManagement.AppService.DTOs
{
    public class CreateAssetRequestReportResponse
    {
        public int AssignmentID { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public DateTime? AssignmentDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int AssetId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public DateTime? PurchaseDate { get; set; }
        public decimal AssetCost { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string AssetStatus { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? AssignedEmployee { get; set; }
    }
}

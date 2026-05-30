namespace AssetManagement.AppService.DTOs
{
    public class AssetReportListResponse
    {
        public int assetId { get; set; }
        public string assetName { get; set; } = string.Empty;
        public string categoryName { get; set; } = string.Empty;
        public string serialNumber { get; set; } = string.Empty;
        public string? purchaseDate { get; set; }
        public decimal assetCost { get; set; }
        public string vendorName { get; set; } = string.Empty;
        public string assetStatus { get; set; } = string.Empty;
        public string healthStatus { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string? propertyName { get; set; }

        public string? assignedEmployee { get; set; }
    }
}
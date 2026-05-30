using System.Collections.Generic;

namespace AssetManagement.AppService.DTOs
{
    public class AssetReportPagedResponse
    {
        public IEnumerable<AssetReportListResponse> data { get; set; } = new List<AssetReportListResponse>();
        public int pageNo { get; set; }
        public int pageSize { get; set; }
        public int totalRecords { get; set; }
        public int totalPages { get; set; }
    }
}
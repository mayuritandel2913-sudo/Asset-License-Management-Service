using AssetManagement.AppService.DTOs;
using AssetManagement.Utility;

namespace AssetManagement.AppService.Contracts
{
    public interface IAdminService
    {
        Task<AssetReportPagedResponse> GetAssetReportListAsync(AssetReportFilterRequest filter);
        Task<byte[]> GetAssetReportPdfAsync(AssetReportFileRequest filter);
        Task<(byte[] Data, string FileName, string ContentType)> GetAssetReportFileAsync(AssetReportFileRequest filter);
        Task<AssetUtilizationPagedResponse> GetAssetUtilizationAsync();
        Task<AssetDowntimePagedResponse> GetAssetDowntimeRateAsync();
        Task<AssetByCategoryPagedResponse> GetAssetsByCategoryAsync();
        Task<IEnumerable<ResponseAssetProperty>> GetPropertiesByCategoryIdAsync(byte categoryId);
    }
}
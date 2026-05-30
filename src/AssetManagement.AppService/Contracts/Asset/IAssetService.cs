using AssetManagement.AppService.DTOs;
using AssetManagement.AppService.DTOs.asset;
using AssetManagement.AppService.DTOs.License;
using AssetManagement.Infrastructure.Entities;

namespace AssetManagement.AppService.Contracts;

public interface IAssetService
{
    Task<Asset> CreateAssetRequestAsync(CreateAssetRequest createAssetRequest, int createdById);
    Task<AssetResponse?> GetAssetByIdAsync(int id);
    

    Task<AssetPagedResponse> GetAllAssetsAsync(string? search, int pageNo);

    Task<AssetAuditLogPagedResponse> GetAssetAuditLogsAsync(int assetId, int pageNo, int pageSize);

    Task<AssetAuditLogDetailPagedResponse> GetAssetAuditLogDetailAsync(int logId);

    Task<AssetResponse?> UpdateAssetRequestAsync(int id, UpdateAssetRequest updateAssetRequest, int modifiedById);

    Task AssignAssetRequestAsync(int assetId, AssignAssetRequest assignAssetRequest, int modifiedById);

    Task UnassignAssetRequestAsync(int assetId, UnassignAssetRequest unassignAssetRequest, int modifiedById);

    Task<AssetPropertyValue> AddAssetPropertyValueAsync(CreateAssetRequestProperty createAssetRequestProperty, int assetId);

    Task<AssetStatusDistributionResponse> GetAssetStatusDistributionAsync(int? categoryId = null, string? categoryName = null);
  
}

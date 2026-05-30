using AssetManagement.Infrastructure.Entities;

namespace AssetManagement.Infrastructure.Contracts;

public interface IAssetRepository
{
    Task<Asset> AddAsyncAsset(Asset asset);

    Task<Asset?> GetByIdAsync(int id);

    Task<IEnumerable<Asset>> GetALLAsync();

    Task<Asset> UpdateAsync(Asset asset);

    Task AddAssignmentAsync(AssignmentDetails assignment);

    Task<AssignmentDetails?> GetActiveAssignmentByAssetAndUserAsync(int assetId, int userId);

    Task UpdateAssignmentAsync(AssignmentDetails assignment);

    Task<AssetProperty> AddAsyncAssetProperty(AssetProperty assetProperties);

    Task<AssetPropertyValue> AddAsyncAssetPropertyValue(AssetPropertyValue assetPropertyValue);
    Task<AssetPropertyValue> UpdateAsyncAssetPropertyValue(AssetPropertyValue assetPropertyValue);

    Task<AssetProperty?> GetAssetPropertyByIdAsync(byte propertyId);

    Task<List<AssetProperty>> GetAssetPropertiesByCategoryIdAsync(byte categoryId);

    Task<List<AssetProperty>> GetAssetPropertiesByCategoryIdsAsync(IEnumerable<byte> categoryIds);

    Task<(List<AssetAuditLog> Logs, int TotalRecords)> GetAssetAuditLogsAsync(int assetId, int pageNo, int pageSize);

    Task<AssetAuditLog?> GetAssetAuditLogByIdAsync(int auditLogId);

    Task<AssetProperty> UpdateAssetPropertyAsync(AssetProperty assetProperty);

    Task AddPropertyAsync(AssetProperty property);

    Task<bool> CategoryExistsAsync(int categoryId);
    
    Task<bool> AnyAssetsForCategoryAsync(byte categoryId);
    Task<byte?> GetAssetStatusIdByNameAsync(string statusName);

    Task<bool> ResourceTypeExistsAsync(int resourceTypeId);

    Task<bool> StatusExistsAsync(int statusId);

    Task<bool> AssetHealthStatusExistsAsync(int assetHealthStatusId);

    Task<bool> UserExistsAsync(int userId);

    Task<bool> SerialNumberExistsAsync(string serialNumber, int? excludeAssetId = null);

    Task<User?> GetUserByIdAsync(int userId);

    Task<IEnumerable<string>> GetAssetHealthStatusNamesAsync();

    Task<IEnumerable<(string StatusName, int AssetCount, decimal Percentage)>> GetAssetStatusDistributionAsync(int? categoryId = null, string? categoryName = null);

    Task<bool> PropertyValueExistsAsync(byte propertyId, string propertyValue, int? excludeAssetId = null);
    
    Task<IEnumerable<string>> GetBlockedHealthStatusNamesAsync();

    Task<IEnumerable<string>> GetAllHealthStatusNamesAsync();
}

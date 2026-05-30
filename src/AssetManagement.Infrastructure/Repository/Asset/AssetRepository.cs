using AssetManagement.Infrastructure.Contracts;
using AssetManagement.Infrastructure.Data;
using AssetManagement.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
 
namespace AssetManagement.Infrastructure.Repository;
 
public class AssetRepository : IAssetRepository
{
    private readonly ApplicationDbContext _applicationDbContext;
 
    public AssetRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }
 
    public async Task AddAssignmentAsync(AssignmentDetails assignment)
    {
        await _applicationDbContext.AssignmentDetails.AddAsync(assignment);
        await _applicationDbContext.SaveChangesAsync();
    }

    public async Task<AssignmentDetails?> GetActiveAssignmentByAssetAndUserAsync(int assetId, int userId)
    {
        return await _applicationDbContext.AssignmentDetails
            .Where(a => a.AssetID == assetId && a.UserID == userId && a.IsActive)
            .OrderByDescending(a => a.AssignmentDate)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAssignmentAsync(AssignmentDetails assignment)
    {
        _applicationDbContext.AssignmentDetails.Update(assignment);
        await _applicationDbContext.SaveChangesAsync();
    }

    public async Task<Asset> AddAsyncAsset(Asset asset)
    {
       await _applicationDbContext.Asset.AddAsync(asset);
       await _applicationDbContext.SaveChangesAsync();
       return asset;
    }
 
    public async Task<AssetProperty> AddAsyncAssetProperty(AssetProperty assetProperties)
    {
        await _applicationDbContext.AssetProperty.AddAsync(assetProperties);
        await _applicationDbContext.SaveChangesAsync();
        return assetProperties;
 
    }

    public async Task<AssetPropertyValue> AddAsyncAssetPropertyValue(AssetPropertyValue assetPropertyValue)
    {
        await _applicationDbContext.AssetPropertyValue.AddAsync(assetPropertyValue);
        await _applicationDbContext.SaveChangesAsync();
        return assetPropertyValue;
    }

    public async Task<AssetPropertyValue> UpdateAsyncAssetPropertyValue(AssetPropertyValue assetPropertyValue)
    {
        _applicationDbContext.AssetPropertyValue.Update(assetPropertyValue);
        await _applicationDbContext.SaveChangesAsync();
        return assetPropertyValue;
    }
 
    public async Task<AssetProperty?> GetAssetPropertyByIdAsync(byte propertyId)
    {
        return await _applicationDbContext.AssetProperty
            .FirstOrDefaultAsync(p => p.PropertyID == propertyId && p.DeletedDate == null);
    }
 
    public async Task<List<AssetProperty>> GetAssetPropertiesByCategoryIdAsync(byte categoryId)
    {
        return await _applicationDbContext.AssetProperty
            .Include(p => p.DataType)
            .Where(p => p.CategoryID == categoryId && p.DeletedDate == null)
            .ToListAsync();
    }
 
    public async Task<List<AssetProperty>> GetAssetPropertiesByCategoryIdsAsync(IEnumerable<byte> categoryIds)
    {
        var ids = categoryIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new List<AssetProperty>();
        }

        return await _applicationDbContext.AssetProperty
            .Include(p => p.DataType)
            .Where(p => ids.Contains(p.CategoryID) && p.DeletedDate == null)
            .ToListAsync();
    }

    public async Task<(List<AssetAuditLog> Logs, int TotalRecords)> GetAssetAuditLogsAsync(int assetId, int pageNo, int pageSize)
    {
        var query = _applicationDbContext.Set<AssetAuditLog>()
            .AsNoTracking()
            .Include(log => log.ActionType)
            .Include(log => log.PerformedBy)
            .Include(log => log.Details)
            .Where(log => log.AssetID == assetId)
            .OrderByDescending(log => log.DateTimestamp)
            .ThenByDescending(log => log.AssetAuditLogID);

        var totalRecords = await query.CountAsync();
        var logs = await query
            .Skip((pageNo - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (logs, totalRecords);
    }

    public async Task<AssetAuditLog?> GetAssetAuditLogByIdAsync(int auditLogId)
    {
        return await _applicationDbContext.Set<AssetAuditLog>()
            .AsNoTracking()
            .Include(log => log.Asset)
            .Include(log => log.Details)
            .FirstOrDefaultAsync(log => log.AssetAuditLogID == auditLogId);
    }

    public async Task<AssetProperty> UpdateAssetPropertyAsync(AssetProperty assetProperty)
    {
        _applicationDbContext.AssetProperty.Update(assetProperty);
        await _applicationDbContext.SaveChangesAsync();
        return assetProperty;
    }
 
 
    public async Task<IEnumerable<Asset>> GetALLAsync()
    {
        return await _applicationDbContext.Asset
            .Include(a => a.Category)
                .ThenInclude(c => c.ResourceType)
            .Include(a => a.AssetStatus)
            .Include(a => a.AssetHealthStatus)
            .Include(a => a.User)
            .Include(a => a.AssetHealthStatus)
            .ToListAsync();
    }
 
 
    public async Task<Asset?> GetByIdAsync(int id)
    {
        return await _applicationDbContext.Asset
            .Include(a => a.Category)
                .ThenInclude(c => c.ResourceType)
            .Include(a => a.AssetStatus)
            .Include(a => a.AssetHealthStatus)
            .Include(a => a.PropertyValues)
            .FirstOrDefaultAsync(a => a.AssetID == id);
    }
 
    public async Task<Asset> UpdateAsync(Asset asset)
    {
        _applicationDbContext.Asset.Update(asset);
        await _applicationDbContext.SaveChangesAsync();
        return asset;
    }
    public async Task AddPropertyAsync(AssetProperty property)
{
    await _applicationDbContext.AssetProperty.AddAsync(property);
    await _applicationDbContext.SaveChangesAsync();
}
 
    public async Task<bool> CategoryExistsAsync(int categoryId)
{
    return await _applicationDbContext.Category.AnyAsync(c => c.CategoryID == categoryId);
}

    public async Task<bool> AnyAssetsForCategoryAsync(byte categoryId)
    {
        return await _applicationDbContext.Asset.AnyAsync(a => a.CategoryID == categoryId && a.DeletedDate == null);
    }

    public async Task<byte?> GetAssetStatusIdByNameAsync(string statusName)
    {
        if (string.IsNullOrWhiteSpace(statusName)) return null;

        var normalized = statusName.Trim();
        return await _applicationDbContext.AssetStatus
            .Where(s => s.AssetStatusName == normalized)
            .Select(s => (byte?)s.AssetStatusID)
            .FirstOrDefaultAsync();
    }
 
    public async Task<bool> ResourceTypeExistsAsync(int resourceTypeId)
    {
        return await _applicationDbContext.ResourceType.AnyAsync(rt => rt.ResourceTypeID == resourceTypeId);
    }
 
    public async Task<bool> StatusExistsAsync(int statusId)
    {
        return await _applicationDbContext.AssetStatus.AnyAsync(s => s.AssetStatusID == statusId);
    }

    public async Task<bool> AssetHealthStatusExistsAsync(int assetHealthStatusId)
    {
        return await _applicationDbContext.AssetHealthStatus.AnyAsync(s => s.AssetHealthStatusID == assetHealthStatusId);
    }
 
    public async Task<bool> UserExistsAsync(int userId)
    {
        return await _applicationDbContext.User.AnyAsync(u => u.UserID == userId && u.IsActive == true && u.DeletedDate == null);
    }
 
    public async Task<bool> SerialNumberExistsAsync(string serialNumber, int? excludeAssetId = null)
    {
        var query = _applicationDbContext.Asset.AsQueryable();
 
        if (excludeAssetId.HasValue)
        {
            var assetId = excludeAssetId.Value;
            return await query.AnyAsync(a => a.SerialNumber == serialNumber && a.AssetID != assetId);
        }
 
        return await query.AnyAsync(a => a.SerialNumber == serialNumber);
    }
 
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _applicationDbContext.User.FirstOrDefaultAsync(u => u.UserID == userId);
    }

    public async Task<IEnumerable<string>> GetAssetHealthStatusNamesAsync()
    {
        return await _applicationDbContext.AssetHealthStatus
            .Select(status => status.AssetHealthStatusName)
            .ToListAsync();
    }

    public async Task<IEnumerable<(string StatusName, int AssetCount, decimal Percentage)>> GetAssetStatusDistributionAsync(int? categoryId = null, string? categoryName = null)
    {
        var query = _applicationDbContext.Asset
            .Where(a => a.DeletedDate == null);

        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            var normalizedCategoryName = categoryName.Trim().ToLowerInvariant();
            query = query.Where(a => a.Category.CategoryName.ToLower() == normalizedCategoryName);
        }
        else if (categoryId.HasValue)
        {
            query = query.Where(a => a.CategoryID == categoryId.Value);
        }

        var totalAssets = await query.CountAsync();

        var statusData = await _applicationDbContext.AssetHealthStatus
            .OrderBy(status => status.AssetHealthStatusID)
            .Select(status => new
            {
                status.AssetHealthStatusName,
                Count = query.Count(asset => asset.AssetHealthStatusID == status.AssetHealthStatusID)
            })
            .ToListAsync();

        return statusData.Select(status => (
            status.AssetHealthStatusName,
            status.Count,
            totalAssets > 0 ? Math.Round((decimal)status.Count / totalAssets * 100, 2) : 0m
        ));
    }

    public async Task<bool> PropertyValueExistsAsync(byte propertyId, string propertyValue, int? excludeAssetId = null)
    {
        if (string.IsNullOrWhiteSpace(propertyValue))
            return false;

        var normalizedValue = propertyValue.Trim().ToLower();

        var query = _applicationDbContext.AssetPropertyValue
            .Include(value => value.Asset)
            .Where(value => value.PropertyID == propertyId && value.Value != null && value.Value.Trim().ToLower() == normalizedValue);

        if (excludeAssetId.HasValue)
        {
            query = query.Where(value => value.AssetID != excludeAssetId.Value);
        }

        return await query.AnyAsync(value => value.Asset.DeletedDate == null);
    }

    public async Task<IEnumerable<string>> GetBlockedHealthStatusNamesAsync()
    {
        return await _applicationDbContext.AssetHealthStatus
            .Where(h => h.AssetHealthStatusName.ToLower() != "good")
            .Select(h => h.AssetHealthStatusName)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetAllHealthStatusNamesAsync()
    {
        return await _applicationDbContext.AssetHealthStatus
            .Select(h => h.AssetHealthStatusName)
            .ToListAsync();
    }
}

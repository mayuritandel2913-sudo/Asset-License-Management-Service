using AssetManagement.Infrastructure.Contracts;
using AssetManagement.Infrastructure.Data;
using AssetManagement.Infrastructure.Entities;
using AssetManagement.Infrastructure.Entities.License;
using AssetManagement.Utility.Resource;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.Repositories
{
    public class MasterServiceRepository : IMasterServiceRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public MasterServiceRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return await _applicationDbContext.Category
                .Include(x => x.ResourceType)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync(string? search = null)
        {
            var query = _applicationDbContext.User
                .Include(x => x.Role)
                .Include(x => x.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var trimmedSearch = search.Trim();
                query = query.Where(user =>
                    ((user.FirstName + " " + user.LastName).Contains(trimmedSearch)) ||
                    user.Email.Contains(trimmedSearch));
            }

            return await query.ToListAsync();
        }

        public async Task<byte?> GetAdminRoleIdAsync()
        {
            return await _applicationDbContext.Role
                .Where(r => r.RoleName == CommonResource.AdminRoleName && r.DeletedDate == null)
                .Select(r => (byte?)r.RoleID)
                .FirstOrDefaultAsync();
        }

        public async Task<List<string>> GetAdminEmailsAsync(byte adminRoleId)
        {
            var admins = await _applicationDbContext.User
                .Where(u => u.RoleID == adminRoleId && u.IsActive == true && u.DeletedDate == null)
                .Select(u => u.Email)
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .ToListAsync();

            return admins.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        public async Task<List<int>> GetAdminUserIdsAsync()
        {
            var adminRoleId = await GetAdminRoleIdAsync();
            if (!adminRoleId.HasValue)
            {
                return new List<int>();
            }

            return await _applicationDbContext.User
                .Where(u => u.RoleID == adminRoleId.Value && u.IsActive == true && u.DeletedDate == null)
                .Select(u => u.UserID)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssetStatus>> GetStatusesAsync()
        {
            return await _applicationDbContext.AssetStatus
                .ToListAsync();
        }

        public async Task<IEnumerable<Role>> GetRolesAsync()
        {
            return await _applicationDbContext.Role
                .ToListAsync();
        }

        public async Task<IEnumerable<DataType>> GetDataTypesAsync()
        {
            return await _applicationDbContext.DataType
                .ToListAsync();
        }

        public async Task<IEnumerable<ResourceType>> GetResourceTypesAsync()
        {
            return await _applicationDbContext.ResourceType
                .ToListAsync();
        }

        public async Task<IEnumerable<AssignmentDetails>> GetAssignmentDetailsAsync()
        {
            return await _applicationDbContext.AssignmentDetails
                .Include(x => x.User)
                .Include(x => x.Asset)
                    .ThenInclude(a => a.AssetStatus)
                .Include(x => x.Asset)
                    .ThenInclude(a => a.Category)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssetReportData>> GetAllAssetsWithAssignmentsAsync()
        {
            var assets = await _applicationDbContext.Asset
                .Where(a => a.DeletedDate == null)
                .Include(a => a.Category)
                .Include(a => a.AssetStatus)
                .Include(a => a.AssetHealthStatus)
                .Include(a => a.AssignmentDetails)
                    .ThenInclude(ad => ad.User)
                .ToListAsync();

            var categoryPropertyMap = await _applicationDbContext.AssetProperty
                .Where(ap => ap.DeletedDate == null)
                .GroupBy(ap => ap.CategoryID)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Where(ap => !string.IsNullOrEmpty(ap.PropertyName))
                          .Select(ap => ap.PropertyName)
                          .ToList());

            var assetIds = assets.Select(a => a.AssetID).ToList();
            var propertyValues = await _applicationDbContext.AssetPropertyValue
                .Include(apv => apv.AssetProperty)
                .Where(apv => assetIds.Contains(apv.AssetID))
                .ToListAsync();

            var propertyValuesMap = propertyValues
                .GroupBy(pv => pv.AssetID)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(pv => pv.AssetProperty.PropertyName, pv => pv.Value ?? string.Empty)
                );

            return assets.Select(a =>
            {
                var firstAssignment = a.AssignmentDetails.OrderByDescending(ad => ad.AssignmentDate).FirstOrDefault();

                var propertyNames = new List<string>();
                if (a.CategoryID > 0 && categoryPropertyMap.ContainsKey(a.CategoryID))
                {
                    propertyNames = categoryPropertyMap[a.CategoryID];
                }

                
                var valueMap = propertyValuesMap.TryGetValue(a.AssetID, out var vm) ? vm : null;

                var propertyPairs = propertyNames.Count > 0
                    ? string.Join(", ", propertyNames.Select(pn =>
                        (valueMap != null && valueMap.TryGetValue(pn, out var val) && !string.IsNullOrWhiteSpace(val))
                            ? $"{pn}:{val}"
                            : pn))
                    : null;

                return new AssetReportData
                {
                    AssetId = a.AssetID,
                    AssetName = a.AssetName,
                    CategoryName = a.Category?.CategoryName,
                    SerialNumber = a.SerialNumber,
                    PurchaseDate = a.PurchaseDate,
                    AssetCost = a.AssetCost ?? 0m,
                    VendorName = a.VendorName,
                    AssetStatus = a.AssetStatus?.AssetStatusName,
                    AssetHealthStatus = a.AssetHealthStatus?.AssetHealthStatusName,
                    Description = a.Description,
                    AssignmentID = firstAssignment?.AssignmentID,
                    EmployeeName = firstAssignment != null ?
                        $"{firstAssignment.User?.FirstName} {firstAssignment.User?.LastName}".Trim() : null,
                    AssignmentDate = firstAssignment?.AssignmentDate,
                    ExpectedReturnDate = firstAssignment?.ExpectedReturnDate,
                    PropertyName = propertyPairs
                };
            });
        }


        public async Task<AssignmentDetails?> GetAssignmentByIdAsync(int assignmentId)
        {
            return await _applicationDbContext.AssignmentDetails
                .Include(x => x.Asset)
                .FirstOrDefaultAsync(x => x.AssignmentID == assignmentId);
        }

        public async Task<byte?> GetAvailableStatusIdAsync()
        {
            return await _applicationDbContext.AssetStatus
                .Where(x => x.AssetStatusName == "Available")
                .Select(x => (byte?)x.AssetStatusID)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<LicenseStatus>> GetAllStatusesAsync()
        {
            return await _applicationDbContext.LicenseStatus
                .ToListAsync();

        }

        public async  Task<IEnumerable<LicenseType>> GetLicenseTypesAsync()
        {
            return await _applicationDbContext.LicenseType
                .ToListAsync();

        }
        public async Task<IEnumerable<LicensePurchaseType>> GetLicensePurchaseTypesAsync()
        {
            return await _applicationDbContext.LicensePurchaseType
                .ToListAsync();

        }
        public async Task<IEnumerable<ReminderConfig>> GetReminderDaysAsync()
        {
            return await _applicationDbContext.ReminderConfig
                .ToListAsync();

        }

        public async Task<IEnumerable<ActionType>> GetActionTypesAsync()
        {
            return await _applicationDbContext.ActionType
                .ToListAsync();

        }
        public async Task<IEnumerable<AssetHealthStatus>> GetAssetHealthStatusesAsync()
        {
            return await _applicationDbContext.AssetHealthStatus
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _applicationDbContext.SaveChangesAsync();
        }
    }
}

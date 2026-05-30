using AssetManagement.Infrastructure.Entities;
using AssetManagement.Infrastructure.Entities.License;

namespace AssetManagement.Infrastructure.Contracts
{
    public interface IMasterServiceRepository
    {
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<IEnumerable<User>> GetAllUsersAsync(string? search = null);
        Task<byte?> GetAdminRoleIdAsync();
        Task<List<string>> GetAdminEmailsAsync(byte adminRoleId);
        Task<List<int>> GetAdminUserIdsAsync();
        Task<IEnumerable<AssetStatus>> GetStatusesAsync();
        Task<IEnumerable<Role>> GetRolesAsync();
        Task<IEnumerable<DataType>> GetDataTypesAsync();
        Task<IEnumerable<ResourceType>> GetResourceTypesAsync();
        Task<IEnumerable<AssignmentDetails>> GetAssignmentDetailsAsync();
        Task<IEnumerable<AssetReportData>> GetAllAssetsWithAssignmentsAsync();
        Task<AssignmentDetails?> GetAssignmentByIdAsync(int assignmentId);
        Task<byte?> GetAvailableStatusIdAsync();
        Task<IEnumerable<LicenseStatus>> GetAllStatusesAsync();
        Task<IEnumerable<LicenseType>> GetLicenseTypesAsync();
        Task<IEnumerable<LicensePurchaseType>> GetLicensePurchaseTypesAsync();
        Task<IEnumerable<ReminderConfig>> GetReminderDaysAsync();
        Task<IEnumerable<ActionType>> GetActionTypesAsync();
        Task<IEnumerable<AssetHealthStatus>> GetAssetHealthStatusesAsync();
        Task SaveChangesAsync();
    }
}

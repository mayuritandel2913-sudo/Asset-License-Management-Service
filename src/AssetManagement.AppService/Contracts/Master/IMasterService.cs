using AssetManagement.AppService.DTOs;
using AssetManagement.AppService.DTOs.License;

namespace AssetManagement.AppService.Contracts
{
    public interface IMasterService
    {
        Task<IEnumerable<CategoryResponse>> GetCategoriesAsync();
        Task<IEnumerable<GetAllUserResponse>> GetAllUsersAsync(string? search = null);

        Task<IEnumerable<GetStatusResponse>> GetStatusesAsync();
        Task<IEnumerable<GetAllRoleResponse>> GetRolesAsync();
        Task<IEnumerable<GetDataTypeResponse>> GetDataTypesAsync();
        Task<IEnumerable<GetResourceTypeResponse>> GetResourceTypesAsync();
        Task<IEnumerable<LicenseStatusResponse>> GetLicenseStatusesAsync();
        Task<IEnumerable<LicenseTypeResponse>> GetLicenseTypesAsync();
        Task<IEnumerable<LicensePurchaseTypeResponse>> GetLicensePurchaseTypesAsync();
        Task<IEnumerable<ReminderDaysResponse>> GetReminderDaysAsync();
        Task<IEnumerable<ActionTypeResponse>> GetActionTypesAsync();
        Task<IEnumerable<AssetHealthStatusResponse>> GetAssetHealthStatusesAsync();
    }
}

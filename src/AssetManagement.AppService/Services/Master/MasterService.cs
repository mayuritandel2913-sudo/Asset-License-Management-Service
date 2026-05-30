using AssetManagement.AppService.Contracts;
using AssetManagement.AppService.DTOs;
using AssetManagement.AppService.DTOs.License;
using AssetManagement.Infrastructure.Contracts;
using AssetManagement.Utility.Exceptions;
using AssetManagement.Utility.Resource;
using Microsoft.Extensions.Logging;

namespace AssetManagement.AppService.Services
{
    public class MasterService : IMasterService
    {
        private readonly IMasterServiceRepository _masterServiceRepository;
        private readonly ILogger<MasterService> _logger;

        public MasterService(IMasterServiceRepository masterServiceRepository, ILogger<MasterService> logger)
        {
            _masterServiceRepository = masterServiceRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<CategoryResponse>> GetCategoriesAsync()
        {
            _logger.LogInformation("Fetching category list.");
            var categories = await _masterServiceRepository.GetCategoriesAsync();
            return categories.Select(x => new CategoryResponse
            {
                CategoryID = x.CategoryID,
                CategoryName = x.CategoryName
            }).ToList();
        }

        public async Task<IEnumerable<GetAllUserResponse>> GetAllUsersAsync(string? search = null)
        {
            string? trimmedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

            if (!string.IsNullOrEmpty(trimmedSearch) && trimmedSearch.Length < 2)
            {
                throw new NotFoundException(CommonResource.NoDataAvailable);
            }

            var users = await _masterServiceRepository.GetAllUsersAsync(trimmedSearch);

            if (!string.IsNullOrEmpty(trimmedSearch) && !users.Any())
            {
                throw new NotFoundException(CommonResource.NoDataAvailable);
            }

            return users.Select(u => new GetAllUserResponse
            {
                UserId = u.UserID,
                Name = $"{u.FirstName} {u.LastName}".Trim(),
                Email = u.Email,
                Role = u.Role?.RoleName ?? string.Empty,
                Status = u.IsActive == true ? "Active" : "Inactive",
                Department = u.Department?.DepartmentName ?? string.Empty
            });
        }

        public async Task<IEnumerable<GetStatusResponse>> GetStatusesAsync()
        {
            _logger.LogInformation("Fetching status list.");
            var statuses = await _masterServiceRepository.GetStatusesAsync();
            return statuses.Select(x => new GetStatusResponse
            {
                StatusID = x.AssetStatusID,
                StatusName = x.AssetStatusName
            }).ToList();
        }

        public async Task<IEnumerable<GetAllRoleResponse>> GetRolesAsync()
        {
            _logger.LogInformation("Fetching role list.");
            var roles = await _masterServiceRepository.GetRolesAsync();
            return roles.Select(x => new GetAllRoleResponse
            {
                RoleId = x.RoleID,
                RoleName = x.RoleName
            }).ToList();
        }

        public async Task<IEnumerable<GetDataTypeResponse>> GetDataTypesAsync()
        {
            _logger.LogInformation("Fetching data type list.");
            var dataTypes = await _masterServiceRepository.GetDataTypesAsync();
            return dataTypes.Select(x => new GetDataTypeResponse
            {
                DataTypeID = x.DataTypeID,
                DataTypeName = x.DataTypeName
            }).ToList();
        }

        public async Task<IEnumerable<GetResourceTypeResponse>> GetResourceTypesAsync()
        {
            _logger.LogInformation("Fetching resource type list.");
            var resourceTypes = await _masterServiceRepository.GetResourceTypesAsync();
            return resourceTypes.Select(x => new GetResourceTypeResponse
            {
                ResourceTypeID = x.ResourceTypeID,
                TypeName = x.TypeName
            }).ToList();
        }

        public async Task<IEnumerable<LicenseStatusResponse>> GetLicenseStatusesAsync()
        {
            _logger.LogInformation("Fetching license status list.");
            var licenseStatuses = await _masterServiceRepository.GetAllStatusesAsync();
            return licenseStatuses.Select(x => new LicenseStatusResponse
            {
                LicenseStatusID = x.LicenseStatusID,
                LicenseStatusName = x.LicenseStatusName
            }).ToList();
        }

        public async Task<IEnumerable<LicenseTypeResponse>> GetLicenseTypesAsync()
        {
            _logger.LogInformation("Fetching license type list.");
            var licenseTypes = await _masterServiceRepository.GetLicenseTypesAsync();
            return licenseTypes.Select(x => new LicenseTypeResponse
            {
                LicenseTypeID = x.LicenseTypeID,
                LicenseTypeName = x.LicenseTypeName
            }).ToList();
        }

        public async Task<IEnumerable<LicensePurchaseTypeResponse>> GetLicensePurchaseTypesAsync()
        {
            _logger.LogInformation("Fetching license purchase type list.");
            var licensePurchaseTypes = await _masterServiceRepository.GetLicensePurchaseTypesAsync();
            return licensePurchaseTypes.Select(x => new LicensePurchaseTypeResponse
            {
                LicensePurchaseTypeID = x.LicensePurchaseTypeID,
                LicensePurchaseTypeName = x.LicensePurchaseTypeName
            }).ToList();
        }

        public async Task<IEnumerable<ReminderDaysResponse>> GetReminderDaysAsync()
        {
             _logger.LogInformation("Fetching reminder days list.");
            var reminderDays = await _masterServiceRepository.GetReminderDaysAsync();
            return reminderDays.Select(x => new ReminderDaysResponse
            {
                ReminderConfigID = x.ReminderConfigID,
                DaysBeforeExpiry = x.DaysBeforeExpiry
            }).ToList();
        }

        public async Task<IEnumerable<ActionTypeResponse>> GetActionTypesAsync()
        {
            _logger.LogInformation("Fetching action type list.");
            var actionTypes = await _masterServiceRepository.GetActionTypesAsync();
            return actionTypes.Select(x => new ActionTypeResponse
            {
                ActionID = x.ActionID,
                ActionName = x.ActionName   
                
            }).ToList();
        }

        public async Task<IEnumerable<AssetHealthStatusResponse>> GetAssetHealthStatusesAsync()
        {
            _logger.LogInformation("Fetching asset health status list.");
            var assetHealthStatuses = await _masterServiceRepository.GetAssetHealthStatusesAsync();
            return assetHealthStatuses.Select(x => new AssetHealthStatusResponse
            {
                AssetHealthStatusID = x.AssetHealthStatusID,
                AssetHealthStatusName = x.AssetHealthStatusName
            }).ToList();
        }
    }
}

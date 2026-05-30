using AssetManagement.Infrastructure.Entities.License;

namespace AssetManagement.Infrastructure.Contracts;

public interface ILicenseRepository
{
    Task<License> AddAsyncLicense(License license);
    Task<bool> LicenseKeyExistsAsync(string licenseKey);
    Task<byte?> GetNewStatusIdAsync();
    Task<List<byte>> GetReminderConfigIdsByDaysAsync(List<int> days);
    Task<bool> LicenseTypeExistsAsync(byte licenseTypeId);
    Task<bool> LicensePurchaseTypeExistsAsync(byte licensePurchaseTypeId);
    Task<string?> GetLicenseTypeNameAsync(byte licenseTypeId);

    Task<IEnumerable<License>> GetLicensesWithAssignmentsAsync(DateTime? startDate = null, DateTime? endDate = null, string? filter = null, string? search = null);
    Task<(IEnumerable<License> Licenses, int TotalRecords)> GetPaginatedLicensesAsync(string? search, string filter, int pageNo, int pageSize);
    Task<License?> GetLicenseByIDAsync(int licenseId);
    Task<(IEnumerable<LicenseAuditLog> Logs, int TotalRecords, string LicenseName)> GetLicenseAuditLogsAsync(int licenseId, int pageNo, int pageSize);

    Task<License?> GetLicenseByIdAsync(int licenseId);
    Task<LicenseAssignment> AddLicenseAssignmentAsync(LicenseAssignment assignment);
    Task<LicenseRenewal?> GetLicenseRenewalByIdAsync(int renewalId);
    Task<List<LicenseRenewal>> GetLicenseRenewalsByLicenseIdAsync(int licenseId);
    Task<LicenseRenewal> AddLicenseRenewalAsync(LicenseRenewal renewal);
    Task UpdateLicenseRenewalAsync(LicenseRenewal renewal);
    Task<List<LicenseRenewal>> GetRenewalsByDateAsync(DateTime renewalDate);
    Task<bool> HasLicenseRenewalAsync(int licenseId);
    Task<List<int>> GetActiveAssigneeIdsByLicenseIdAsync(int licenseId);
    Task<bool> CheckAvailableSeatsAsync(int licenseId, int assigneeCount);
    Task<bool> UserExistsAsync(int userId);
    Task<bool> IsUserActiveAsync(int userId);
    Task<bool> IsUserAlreadyAssignedAsync(int licenseId, int userId);
    Task<LicenseAssignment?> GetLicenseAssignmentAsync(int licenseId, int userId);
    Task UnassignLicenseAsync(int licenseId, int userId, int unassignedById);
    Task<int> GetAssignedSeatsCountAsync(int licenseId);
    Task UpdateLicenseAsync(License license);
    Task<bool> LicenseKeyExistsForOtherLicenseAsync(string licenseKey, int licenseId);
    Task DeleteLicenseRemindersAsync(int licenseId);
    Task<byte?> GetStatusIdByNameAsync(string statusName);
    Task<bool> ReminderConfigExistsAsync(byte reminderConfigId);
    Task<LicenseAuditLog?> GetLicenseAuditLogDetailsAsync(int logId);
    Task<(int Assigned, int Unassigned)> GetLicenseUtilizationAsync(string? licenseType = null);
    Task<(int New, int Active, int Expired)> GetLicenseStatusOverviewAsync(string? licenseType = null);
    Task<IEnumerable<(string Month, decimal TotalCost)>> GetLicenseCostOvertimeAsync(int year, string? licenseType = null);
    Task<IEnumerable<(string LicenseName, DateTime ExpiryDate, int DaysRemaining)>> GetUpcomingLicenseExpirationsAsync(string? licenseType = null);
    Task<IEnumerable<int>> GetAvailableLicenseYearsAsync();

    // Get all license reminders with related license and reminder configuration
    Task<List<LicenseReminder>> GetAllLicenseRemindersAsync();

}

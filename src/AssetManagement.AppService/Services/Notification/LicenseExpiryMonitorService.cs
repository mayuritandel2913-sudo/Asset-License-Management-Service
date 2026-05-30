using AssetManagement.AppService.Contracts;
using AssetManagement.Infrastructure.Contracts;
using AssetManagement.Infrastructure.Entities.Notification;
using AssetManagement.Utility.Resource;
using NotificationEntity = AssetManagement.Infrastructure.Entities.Notification.Notification;
using Microsoft.Extensions.Logging;

namespace AssetManagement.AppService.Services.Notification;

public class LicenseExpiryMonitorService : ILicenseExpiryMonitorService
{
    private readonly ILicenseRepository _licenseRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IMasterServiceRepository _masterRepository;
    private readonly ILogger<LicenseExpiryMonitorService> _logger;

    public LicenseExpiryMonitorService(
        ILicenseRepository licenseRepository,
        INotificationRepository notificationRepository,
        IMasterServiceRepository masterRepository,
        ILogger<LicenseExpiryMonitorService> logger)
    {
        _licenseRepository = licenseRepository;
        _notificationRepository = notificationRepository;
        _masterRepository = masterRepository;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        try
        {
            _logger.LogInformation("License Expiry Monitor Service Started at {StartedAt}", DateTimeOffset.Now.ToString("MM-dd-yyyy HH:mm:ss zzz"));
            await CheckAndCreateNotificationsAsync();
            _logger.LogInformation("License Expiry Monitor Service Completed at {CompletedAt}", DateTimeOffset.Now.ToString("MM-dd-yyyy HH:mm:ss zzz"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in License Expiry Monitor Service");
        }
    }

    public async Task CheckAndCreateNotificationsAsync()
    {
        try
        {
            var queueStatusId = await _notificationRepository.GetNotificationStatusIdByNameAsync(CommonResource.QueueNotificationStatusName);
            if (!queueStatusId.HasValue)
            {
                _logger.LogWarning("Queue status not found in NotificationStatus master data");
                return;
            }

            var licenseExpiringTypeId = await _notificationRepository.GetNotificationTypeIdByNameAsync(CommonResource.LicenseExpiringNotificationTypeName);
            if (!licenseExpiringTypeId.HasValue)
            {
                _logger.LogWarning("LicenseExpiring type not found in NotificationType master data");
                return;
            }

            var licenseExpiredTypeId = await _notification_repository_getexpiredtype();
            if (!licenseExpiredTypeId.HasValue)
            {
                _logger.LogWarning("LicenseExpired type not found in NotificationType master data");
                return;
            }

            var reminders = await _licenseRepository.GetAllLicenseRemindersAsync();
            _logger.LogInformation("Found {Count} licenses with reminder configurations", reminders.Count);

            foreach (var lr in reminders)
            {
                var license = lr.License;
                if (!license.ExpiryDate.HasValue || lr.ReminderConfig == null)
                {
                    continue;
                }

                var today = DateTime.Today;
                var hasRenewal = await _licenseRepository.HasLicenseRenewalAsync(license.LicenseID);
                if (hasRenewal && license.ExpiryDate.Value.Date >= today)
                {
                    _logger.LogInformation(
                        "Skipping notification creation for LicenseID {LicenseId} because a renewal exists and the license is not expired.",
                        license.LicenseID);
                    continue;
                }

                var daysUntilExpiry = (license.ExpiryDate.Value.Date - DateTime.Today).Days;

                if (daysUntilExpiry == lr.ReminderConfig.DaysBeforeExpiry)
                {
                    await CreateNotificationsForTypeAsync(license, licenseExpiringTypeId.Value, queueStatusId.Value);
                }

                if (daysUntilExpiry == 0)
                {
                    await CreateNotificationsForTypeAsync(license, licenseExpiredTypeId.Value, queueStatusId.Value);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking and creating notifications");
        }
    }

    private async Task CreateNotificationsForTypeAsync(
        AssetManagement.Infrastructure.Entities.License.License license,
        byte notificationTypeId,
        byte queueStatusId)
    {
        var activeAssigneeIds = await _licenseRepository.GetActiveAssigneeIdsByLicenseIdAsync(license.LicenseID);

        var adminIds = await _masterRepository.GetAdminUserIdsAsync();

        if (adminIds.Count == 0)
        {
            _logger.LogWarning("No users found for admin role {AdminRoleName}.", CommonResource.AdminRoleName);
        }

        var createdByUserId = adminIds.FirstOrDefault();
        if (createdByUserId <= 0)
        {
            createdByUserId = activeAssigneeIds.FirstOrDefault();
        }

        if (createdByUserId <= 0)
        {
            _logger.LogWarning("Skipping notification creation for LicenseID {LicenseId} because no valid user exists for CreatedByID.", license.LicenseID);
            return;
        }

        var candidateEmployeeIds = activeAssigneeIds
            .Concat(adminIds)
            .Distinct()
            .ToList();

        var existingEmployeeIds = await _notificationRepository
            .GetExistingNotificationEmployeeIdsAsync(license.LicenseID, notificationTypeId, candidateEmployeeIds);

        var missingEmployeeIds = candidateEmployeeIds
            .Where(employeeId => !existingEmployeeIds.Contains(employeeId))
            .ToList();

        foreach (var employeeId in missingEmployeeIds)
        {
            var notification = new NotificationEntity
            {
                NotificationStatusId = queueStatusId,
                NotificationTypeId = notificationTypeId,
                LicenseId = license.LicenseID,
                EmployeeId = employeeId,
                CreatedByID = createdByUserId,
                CreatedDate = DateTime.UtcNow
            };

            await _notificationRepository.AddNotificationAsync(notification);
        }

        _logger.LogInformation(
            "Processed notifications for LicenseID {LicenseId}, Type {NotificationTypeId}: created {CreatedCount} records.",
            license.LicenseID,
            notificationTypeId,
            missingEmployeeIds.Count);
    }

    private async Task<byte?> _notification_repository_getexpiredtype()
    {
        return await _notificationRepository.GetNotificationTypeIdByNameAsync("LicenseExpired");
    }
}

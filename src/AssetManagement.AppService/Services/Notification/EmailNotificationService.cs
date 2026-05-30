using AssetManagement.AppService.Contracts;
using AssetManagement.Infrastructure.Contracts;
using AssetManagement.Infrastructure.Entities.Notification;
using NotificationEntity = AssetManagement.Infrastructure.Entities.Notification.Notification;
using AssetManagement.Utility.Resource;
using AssetManagement.Utility.DTOs.Notification;
using AssetManagement.Utility.Notification;
using AssetManagement.Utility.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AssetManagement.AppService.Services;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMasterServiceRepository _masterRepository;

    public EmailNotificationService(
        ILogger<EmailNotificationService> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        IMasterServiceRepository masterRepository)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _masterRepository = masterRepository;
    }

    public async Task ExecuteAsync()
    {
        try
        {
            _logger.LogInformation("Email Notification Service Started at {StartedAt}", DateTimeOffset.Now.ToString("MM-dd-yyyy HH:mm:ss zzz"));
            await SendPendingNotificationsAsync();
            _logger.LogInformation("Email Notification Service Completed at {CompletedAt}", DateTimeOffset.Now.ToString("MM-dd-yyyy HH:mm:ss zzz"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in Email Notification Service");
        }
    }

    public async Task SendNotificationEmailAsync(int notificationId)
    {
        using var scope = _serviceProvider.CreateScope();
        var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        await SendNotificationEmailAsync(notificationId, notificationRepository);
    }

    public async Task SendPendingNotificationsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

        try
        {
            var queueStatusId = await GetNotificationStatusIdAsync(notificationRepository, CommonResource.QueueNotificationStatusName);
            if (!queueStatusId.HasValue)
            {
                _logger.LogWarning("Notification status {StatusName} not found", CommonResource.QueueNotificationStatusName);
                return;
            }

            var pendingNotifications = await notificationRepository.GetNotificationsByStatusAsync(queueStatusId.Value);
            _logger.LogInformation("Found {Count} pending notifications to send", pendingNotifications.Count);

            var groups = pendingNotifications.GroupBy(n => new NotificationGroupKey(
                n.LicenseId,
                n.NotificationType?.NotificationTypeName ?? string.Empty));

            foreach (var notificationGroup in groups)
            {
                await ProcessNotificationGroupAsync(notificationGroup, notificationRepository);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending pending notifications");
        }
    }

    private async Task SendNotificationEmailAsync(int notificationId, INotificationRepository notificationRepository)
    {
        try
        {
            var notification = await notificationRepository.GetNotificationByIdAsync(notificationId);
            if (notification == null)
            {
                _logger.LogWarning("Notification {NotificationId} not found", notificationId);
                return;
            }

            if (notification.License == null || notification.Employee == null)
            {
                _logger.LogWarning("Notification {NotificationId} missing license or employee details", notificationId);
                return;
            }

            var daysLeft = GetDaysLeft(notification.License.ExpiryDate);
            if (ShouldSkipExpiringNotification(notification.NotificationType?.NotificationTypeName, notification.License, daysLeft))
            {
                _logger.LogInformation(
                    "Skipping stale expiring notification {NotificationId} for LicenseID {LicenseId}. DaysLeft={DaysLeft}",
                    notificationId,
                    notification.License.LicenseID,
                    daysLeft);

                var failedStatusId = await GetNotificationStatusIdAsync(notificationRepository, CommonResource.FailedNotificationStatusName);
                if (failedStatusId.HasValue)
                {
                    await notificationRepository.UpdateNotificationStatusAsync(notificationId, failedStatusId.Value);
                }

                return;
            }

            var (subject, body) = BuildEmailContent(notification.License, notification.NotificationType?.NotificationTypeName, daysLeft, notification.License.ExpiryDate ?? DateTime.Now);

            var emailRequest = BuildSendEmailRequest(notification.Employee.Email ?? string.Empty, subject, body);
            var emailSent = await EmailManager.SendEmailAsync(emailRequest);

            if (!emailSent)
            {
                _logger.LogError("Failed to send email to {ToEmail}", emailRequest.ToEmail);
            }

            var newStatusId = await GetNotificationStatusIdAsync(notificationRepository, emailSent ? CommonResource.SentNotificationStatusName : CommonResource.FailedNotificationStatusName);
            if (!newStatusId.HasValue)
            {
                _logger.LogWarning("Unable to update notification {NotificationId} because a status record was not found", notificationId);
                return;
            }

            await notificationRepository.UpdateNotificationStatusAsync(notificationId, newStatusId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email for notification {NotificationId}", notificationId);
            var failedStatusId = await GetNotificationStatusIdAsync(notificationRepository, CommonResource.FailedNotificationStatusName);
            if (failedStatusId.HasValue)
            {
                await notificationRepository.UpdateNotificationStatusAsync(notificationId, failedStatusId.Value);
            }
        }
    }

    private async Task ProcessNotificationGroupAsync(
        IGrouping<NotificationGroupKey, NotificationEntity> notificationGroup,
        INotificationRepository notificationRepository)
    {
        try
        {
            var first = notificationGroup.First();
            var license = first.License;
            if (license == null || !license.ExpiryDate.HasValue)
            {
                return;
            }

            var adminRoleId = await _masterRepository.GetAdminRoleIdAsync();
            if (!adminRoleId.HasValue)
            {
                _logger.LogWarning("Role {RoleName} not found", CommonResource.AdminRoleName);
                return;
            }

            var adminEmails = await _masterRepository.GetAdminEmailsAsync(adminRoleId.Value);
            if (adminEmails.Count == 0)
            {
                _logger.LogWarning("No IT admin found to send license notifications for license {LicenseId}", license.LicenseID);
                return;
            }

            var employeeEmails = GetEmployeeEmails(notificationGroup, adminRoleId.Value);
            var daysLeft = GetDaysLeft(license.ExpiryDate);

            if (ShouldSkipExpiringNotification(first.NotificationType?.NotificationTypeName, license, daysLeft))
            {
                _logger.LogInformation(
                    "Skipping stale expiring notification group for LicenseID {LicenseId}. DaysLeft={DaysLeft}",
                    license.LicenseID,
                    daysLeft);

                var failedStatusId = await GetNotificationStatusIdAsync(notificationRepository, CommonResource.FailedNotificationStatusName);
                if (!failedStatusId.HasValue)
                {
                    return;
                }

                foreach (var notification in notificationGroup)
                {
                    await notificationRepository.UpdateNotificationStatusAsync(notification.NotificationId, failedStatusId.Value);
                }

                return;
            }

            var (subject, body) = BuildEmailContent(license, first.NotificationType?.NotificationTypeName, daysLeft, license.ExpiryDate.Value);

            var emailRequest = BuildSendEmailRequest(string.Join(",", adminEmails), subject, body, employeeEmails);
            var emailSent = await EmailManager.SendEmailAsync(emailRequest);

            if (!emailSent)
            {
                _logger.LogError("Failed to send email to admins for license {LicenseId}", license.LicenseID);
            }

            var newStatusName = emailSent ? CommonResource.SentNotificationStatusName : CommonResource.FailedNotificationStatusName;
            var newStatusId = await GetNotificationStatusIdAsync(notificationRepository, newStatusName);
            if (!newStatusId.HasValue)
            {
                _logger.LogWarning("Notification status {StatusName} not found while updating license {LicenseId}", newStatusName, license.LicenseID);
                return;
            }

            foreach (var notification in notificationGroup)
            {
                await notificationRepository.UpdateNotificationStatusAsync(notification.NotificationId, newStatusId.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending aggregated notification for license {LicenseId} and type {NotificationType}", notificationGroup.Key.LicenseId, notificationGroup.Key.NotificationTypeName);
            var failedStatusId = await GetNotificationStatusIdAsync(notificationRepository, CommonResource.FailedNotificationStatusName);
            if (!failedStatusId.HasValue)
            {
                return;
            }

            foreach (var notification in notificationGroup)
            {
                await notificationRepository.UpdateNotificationStatusAsync(notification.NotificationId, failedStatusId.Value);
            }
        }
    }

    private static int GetDaysLeft(DateTime? expiryDate)
    {
        if (!expiryDate.HasValue)
        {
            return 0;
        }

        return (expiryDate.Value.Date - DateTime.Now.Date).Days;
    }

    private static bool ShouldSkipExpiringNotification(
        string? notificationTypeName,
        AssetManagement.Infrastructure.Entities.License.License license,
        int daysLeft)
    {
        if (!string.Equals(notificationTypeName, CommonResource.LicenseExpiringNotificationTypeName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (daysLeft < 0)
        {
            return true;
        }

        var configuredReminderDays = license.LicenseReminders
            .Where(lr => lr.ReminderConfig != null)
            .Select(lr => lr.ReminderConfig.DaysBeforeExpiry)
            .Distinct()
            .ToHashSet();

        return configuredReminderDays.Count == 0 || !configuredReminderDays.Contains(daysLeft);
    }

    private static (string Subject, string Body) BuildEmailContent(
        AssetManagement.Infrastructure.Entities.License.License license,
        string? typeName,
        int daysLeft,
        DateTime expiryDate)
    {
        if (string.Equals(typeName, "LicenseExpired", StringComparison.OrdinalIgnoreCase) || daysLeft < 0)
        {
            var overdue = new LicenseReminderEmailTemplateValue
            {
                LicenseName = license.LicenseName,
                DaysValue = Math.Abs(daysLeft),
                ExpiryDate = expiryDate
            };

            return (
                LicenseReminderEmailTemplate.BuildExpiredSubject(overdue),
                LicenseReminderEmailTemplate.BuildExpiredTemplate(overdue));
        }

        var expiring = new LicenseReminderEmailTemplateValue
        {
            LicenseName = license.LicenseName,
            DaysValue = daysLeft,
            ExpiryDate = expiryDate
        };

        return (
            LicenseReminderEmailTemplate.BuildExpiringSubject(expiring),
            LicenseReminderEmailTemplate.BuildExpiringTemplate(expiring));
    }

    private static List<string> GetEmployeeEmails(IEnumerable<NotificationEntity> notifications, byte adminRoleId)
    {
        return notifications
            .Where(n => !string.IsNullOrWhiteSpace(n.Employee?.Email) && n.Employee?.RoleID != adminRoleId)
            .Select(n => n.Employee!.Email!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static async Task<byte?> GetNotificationStatusIdAsync(
        INotificationRepository notificationRepository,
        string statusName)
    {
        return await notificationRepository.GetNotificationStatusIdByNameAsync(statusName);
    }

    private SendEmailRequest BuildSendEmailRequest(string toEmail, string subject, string body, List<string>? ccEmails = null)
    {
        var smtpHost = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
        var smtpPortStr = _configuration["Smtp:Port"] ?? "587";
        var enableSslStr = _configuration["Smtp:EnableSsl"] ?? "true";
        var smtpUsername = _configuration["Smtp:UserName"] ?? string.Empty;
        var smtpPassword = _configuration["Smtp:Password"] ?? string.Empty;
        var fromEmail = _configuration["Smtp:FromEmail"] ?? "noreply@assetmanagement.com";
        var fromName = _configuration["Smtp:FromName"] ?? "IT Team";

        if (!int.TryParse(smtpPortStr, out var smtpPort))
        {
            smtpPort = 587;
        }

        var enableSsl = bool.TryParse(enableSslStr, out var sslResult) && sslResult;

        return new SendEmailRequest
        {
            ToEmail = toEmail,
            Subject = subject,
            Body = body,
            CcEmails = ccEmails ?? new List<string>(),
            SmtpHost = smtpHost,
            SmtpPort = smtpPort,
            EnableSsl = enableSsl,
            SmtpUsername = smtpUsername,
            SmtpPassword = smtpPassword,
            FromEmail = fromEmail,
            FromName = fromName
        };
    }

    private sealed record NotificationGroupKey(int LicenseId, string NotificationTypeName);
}

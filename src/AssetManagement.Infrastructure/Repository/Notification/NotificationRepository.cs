using AssetManagement.Infrastructure.Contracts;
using AssetManagement.Infrastructure.Data;
using AssetManagement.Infrastructure.Entities.Notification;
using AssetManagement.Utility.Resource;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.Repository;

public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public NotificationRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }


    public async Task<Notification> AddNotificationAsync(Notification notification)
    {
        _applicationDbContext.Notification.Add(notification);
        await _applicationDbContext.SaveChangesAsync();
        return notification;
    }


    public async Task<List<Notification>> GetNotificationsByStatusAsync(byte statusId)
    {
        return await _applicationDbContext.Notification
            .Include(n => n.License)
                .ThenInclude(l => l!.LicenseReminders)
                    .ThenInclude(lr => lr.ReminderConfig)
            .Include(n => n.Employee)
            .Include(n => n.NotificationStatus)
            .Include(n => n.NotificationType)
                .Where(n => n.NotificationStatusId == statusId && n.DeletedDate == null)
            .ToListAsync();
    }

    public async Task<Notification?> GetNotificationByIdAsync(int notificationId)
    {
        return await _applicationDbContext.Notification
            .Include(n => n.License)
                .ThenInclude(l => l!.LicenseReminders)
                    .ThenInclude(lr => lr.ReminderConfig)
            .Include(n => n.Employee)
            .Include(n => n.NotificationStatus)
            .Include(n => n.NotificationType)
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.DeletedDate == null);
    }

 
    public async Task<bool> UpdateNotificationStatusAsync(int notificationId, byte newStatusId)
    {
        var notification = await _applicationDbContext.Notification
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.DeletedDate == null);

        if (notification == null)
            return false;

        notification.NotificationStatusId = newStatusId;
        _applicationDbContext.Notification.Update(notification);
        await _applicationDbContext.SaveChangesAsync();
        return true;
    }

    public async Task<List<Notification>> GetPendingNotificationsWithDetailsAsync()
    {
        return await _applicationDbContext.Notification
            .Include(n => n.License)
                .ThenInclude(l => l!.LicenseReminders)
                    .ThenInclude(lr => lr.ReminderConfig)
            .Include(n => n.Employee)
            .Include(n => n.NotificationStatus)
            .Include(n => n.NotificationType)
                .Where(n => n.NotificationStatus!.NotificationStatusName == CommonResource.QueueNotificationStatusName && n.DeletedDate == null)
            .ToListAsync();
    }

    public async Task<byte?> GetNotificationStatusIdByNameAsync(string statusName)
    {
        return await _applicationDbContext.NotificationStatus
            .Where(ns => ns.NotificationStatusName == statusName)
            .Select(ns => (byte?)ns.NotificationStatusID)
            .FirstOrDefaultAsync();
    }

    public async Task<byte?> GetNotificationTypeIdByNameAsync(string typeName)
    {
        return await _applicationDbContext.NotificationType
            .Where(nt => nt.NotificationTypeName == typeName)
            .Select(nt => (byte?)nt.NotificationTypeID)
            .FirstOrDefaultAsync();
    }

   
    public async Task<bool> NotificationExistsAsync(int licenseId, byte notificationTypeId)
    {
        return await _applicationDbContext.Notification
            .AnyAsync(n => n.LicenseId == licenseId 
                && n.NotificationTypeId == notificationTypeId
                && n.DeletedDate == null);
    }

    public async Task<bool> NotificationExistsForEmployeeAsync(int licenseId, int employeeId, byte notificationTypeId)
    {
        return await _applicationDbContext.Notification
            .AnyAsync(n => n.LicenseId == licenseId
                && n.NotificationTypeId == notificationTypeId
                && n.EmployeeId == employeeId
                && n.DeletedDate == null);
    }

    public async Task<HashSet<int>> GetExistingNotificationEmployeeIdsAsync(int licenseId, byte notificationTypeId, IEnumerable<int> employeeIds)
    {
        var employeeIdList = employeeIds.Distinct().ToList();
        if (employeeIdList.Count == 0)
        {
            return new HashSet<int>();
        }

        var existingEmployeeIds = await _applicationDbContext.Notification
            .Where(n => n.LicenseId == licenseId
                && n.NotificationTypeId == notificationTypeId
                && employeeIdList.Contains(n.EmployeeId)
                && n.DeletedDate == null)
            .Select(n => n.EmployeeId)
            .Distinct()
            .ToListAsync();

        return existingEmployeeIds.ToHashSet();
    }

    public async Task<int> SoftDeleteNotificationsByLicenseTypeAndStatusAsync(int licenseId, byte notificationTypeId, byte statusId, int deletedById)
    {
        var notifications = await _applicationDbContext.Notification
            .Where(n =>
                n.LicenseId == licenseId &&
                n.NotificationTypeId == notificationTypeId &&
                n.NotificationStatusId == statusId &&
                n.DeletedDate == null)
            .ToListAsync();

        if (notifications.Count == 0)
        {
            return 0;
        }

        var deletedOn = DateTime.UtcNow;
        foreach (var notification in notifications)
        {
            notification.DeletedByID = deletedById;
            notification.DeletedDate = deletedOn;
            notification.ModifiedByID = deletedById;
            notification.ModifiedDate = deletedOn;
        }

        _applicationDbContext.Notification.UpdateRange(notifications);
        await _applicationDbContext.SaveChangesAsync();
        return notifications.Count;
    }
}

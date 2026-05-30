using AssetManagement.Infrastructure.Entities.Notification;

namespace AssetManagement.Infrastructure.Contracts;


public interface INotificationRepository
{
   
    Task<Notification> AddNotificationAsync(Notification notification);

    
    Task<List<Notification>> GetNotificationsByStatusAsync(byte statusId);

    Task<Notification?> GetNotificationByIdAsync(int notificationId);


    Task<bool> UpdateNotificationStatusAsync(int notificationId, byte newStatusId);

    
    Task<List<Notification>> GetPendingNotificationsWithDetailsAsync();


    Task<byte?> GetNotificationStatusIdByNameAsync(string statusName);

  
    Task<byte?> GetNotificationTypeIdByNameAsync(string typeName);


    Task<bool> NotificationExistsAsync(int licenseId, byte notificationTypeId);


    Task<bool> NotificationExistsForEmployeeAsync(int licenseId, int employeeId, byte notificationTypeId);


    Task<HashSet<int>> GetExistingNotificationEmployeeIdsAsync(int licenseId, byte notificationTypeId, IEnumerable<int> employeeIds);

    Task<int> SoftDeleteNotificationsByLicenseTypeAndStatusAsync(int licenseId, byte notificationTypeId, byte statusId, int deletedById);
}

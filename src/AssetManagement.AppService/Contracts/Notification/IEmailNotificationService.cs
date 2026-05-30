namespace AssetManagement.AppService.Contracts;

public interface IEmailNotificationService
{
 
    Task ExecuteAsync();


    Task SendNotificationEmailAsync(int notificationId);


    Task SendPendingNotificationsAsync();
}

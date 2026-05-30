namespace AssetManagement.AppService.Contracts;

public interface ILicenseExpiryMonitorService
{
    Task ExecuteAsync();
    Task CheckAndCreateNotificationsAsync();
}

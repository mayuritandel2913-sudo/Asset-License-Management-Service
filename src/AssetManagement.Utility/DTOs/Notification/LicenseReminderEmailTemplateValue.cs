namespace AssetManagement.Utility.DTOs.Notification;

public class LicenseReminderEmailTemplateValue
{
    public string LicenseName { get; set; } = string.Empty;
    public int DaysValue { get; set; }
    public DateTime ExpiryDate { get; set; }
}

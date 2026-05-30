using System.Text;
using AssetManagement.Utility.DTOs.Notification;

namespace AssetManagement.Utility.Templates;

public static class LicenseReminderEmailTemplate
{
    public static string BuildExpiringSubject(LicenseReminderEmailTemplateValue reminder)
    {
        var daysWord = reminder.DaysValue == 1 ? "day" : "days";
        return $"{reminder.LicenseName} expires in {reminder.DaysValue} {daysWord}";
    }

    public static string BuildExpiredSubject(LicenseReminderEmailTemplateValue reminder)
    {
        return $"{reminder.LicenseName} has expired.";
    }

    public static string BuildExpiringTemplate(LicenseReminderEmailTemplateValue reminder)
    {
        var daysWord = reminder.DaysValue == 1 ? "day" : "days";
        StringBuilder body = new();

        body.Append($@"
                <p>Hi</p>

                <p>
                    This is to inform you that {reminder.LicenseName} expires in {reminder.DaysValue} {daysWord} expiry date: {reminder.ExpiryDate:MM-dd-yyyy}.
                </p>

                <p>
                    Kindly review and proceed with the necessary actions.
                </p>

                <p>Regards,</p>
                <p>IT Team</p>
            ");

        return body.ToString().Trim();
    }

    public static string BuildExpiredTemplate(LicenseReminderEmailTemplateValue reminder)
    {
        StringBuilder body = new();

        body.Append($@"
                <p>Hi there,</p>

                <p>
                    We are writing to let you know that {reminder.LicenseName} expired on {reminder.ExpiryDate:MM-dd-yyyy}.
                </p>

                <p>
                    Please look into this and take any required steps.
                </p>

                <p>Best regards,</p>
                <p>The IT Team</p>
            ");

        return body.ToString().Trim();
    }
}

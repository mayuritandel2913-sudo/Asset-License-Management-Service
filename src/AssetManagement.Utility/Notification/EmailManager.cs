using AssetManagement.Utility.DTOs.Notification;
using System.Net;
using System.Net.Mail;

namespace AssetManagement.Utility.Notification;

public static class EmailManager
{
    public static async Task<bool> SendEmailAsync(SendEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ToEmail))
        {
            return false;
        }

        try
        {
            using var client = new SmtpClient(request.SmtpHost, request.SmtpPort)
            {
                EnableSsl = request.EnableSsl
            };

            if (!string.IsNullOrEmpty(request.SmtpUsername) && !string.IsNullOrEmpty(request.SmtpPassword))
            {
                client.Credentials = new NetworkCredential(request.SmtpUsername, request.SmtpPassword);
            }

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(request.FromEmail, request.FromName),
                Subject = request.Subject,
                Body = request.Body,
                IsBodyHtml = true
            };

            foreach (var addr in request.ToEmail.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                mailMessage.To.Add(addr.Trim());
            }

            foreach (var cc in request.CcEmails.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(cc))
                {
                    mailMessage.CC.Add(cc.Trim());
                }
            }

            await client.SendMailAsync(mailMessage);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

using System.Text.Json.Serialization;

namespace AssetManagement.AppService.DTOs.License;

public class ReminderConfigRequest
{
    [JsonPropertyName("reminderConfigTypeID")]
    public byte ReminderConfigTypeID { get; set; }
}
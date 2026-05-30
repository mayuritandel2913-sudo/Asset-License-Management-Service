using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AssetManagement.AppService.DTOs;

public class UnassignAssetRequest
{
    [Required]
    public int UserID { get; set; }

    [JsonPropertyName("remark")]
    [MaxLength(100, ErrorMessage = "Remark cannot exceed 100 characters.")]
    public string? Remark { get; set; }
}

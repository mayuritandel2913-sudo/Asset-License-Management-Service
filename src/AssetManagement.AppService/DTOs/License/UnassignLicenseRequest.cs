using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AssetManagement.Utility.Resource;

namespace AssetManagement.AppService.DTOs.License;

public class UnassignLicenseRequest
{
    [JsonPropertyName("userId")]
    [Required(ErrorMessage = CommonResource.UserId_Required)]
    [Range(1, int.MaxValue, ErrorMessage = CommonResource.UserId_Invalid)]
    public int UserId { get; set; }
}

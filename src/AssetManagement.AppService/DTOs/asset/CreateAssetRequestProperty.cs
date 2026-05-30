using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AssetManagement.AppService.DTOs.User;


namespace AssetManagement.AppService.DTOs;

public class CreateAssetRequestProperty : BaseUserRequest
{
    [JsonPropertyName("propertyId")]
    [Range(1, int.MaxValue, ErrorMessage = "propertyId must be greater than 0.")]
    public int PropertyID { get; set; }

    [JsonPropertyName("propertyValue")]
    [StringLength(255)]
    public string? PropertyValue { get; set; }
}

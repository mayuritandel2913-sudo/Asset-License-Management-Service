using System.ComponentModel.DataAnnotations;

namespace AssetManagement.AppService.DTOs
{
    public class GenerateTokenRequest
    {
        [Required(ErrorMessage = "Please enter the email")]
        [RegularExpression(
            @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;
    }
}
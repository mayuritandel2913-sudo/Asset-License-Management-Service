using AssetManagement.Utility.Resource;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AssetManagement.AppService.DTOs
{
    public class CreateCategoryRequest
    {
        [Required(ErrorMessage = CommonResource.CategoryNameRequired)]
        [MinLength(2, ErrorMessage = CommonResource.CategoryNameMinLength)]
        [MaxLength(50, ErrorMessage = CommonResource.CategoryNameMaxLength)]
        [RegularExpression("^(?!\\d+$).+$", ErrorMessage = CommonResource.CategoryNameCannotBeOnlyDigits)]
        public string CategoryName { get; set; } = null!;

        [Required(ErrorMessage = CommonResource.ResourceTypeIdRequired)]
        [Range(1, byte.MaxValue, ErrorMessage = CommonResource.ResourceTypeIdRequired)]
        public byte ResourceTypeID { get; set; }

        [JsonPropertyName("properties")]
        public List<CreateCategoryPropertyRequest>? CategoryProperties { get; set; }
    }
}
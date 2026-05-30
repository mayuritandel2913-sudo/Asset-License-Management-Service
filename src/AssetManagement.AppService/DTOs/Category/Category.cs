using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AssetManagement.Utility.Resource;

namespace AssetManagement.AppService.DTOs;

public class UpdateCategoryRequest
{
    [Required(ErrorMessage = CommonResource.CategoryNameRequired)]
    
    [MinLength(2, ErrorMessage = CommonResource.CategoryNameMinLength)]
    [MaxLength(50, ErrorMessage = CommonResource.CategoryNameMaxLength)]
    [RegularExpression("^(?!\\d+$).+$", ErrorMessage = CommonResource.CategoryNameCannotBeOnlyDigits)]
    public string CategoryName { get; set; } = string.Empty;

    [Required(ErrorMessage = CommonResource.ResourceTypeIdRequired)]
    [Range(1, byte.MaxValue, ErrorMessage = CommonResource.ResourceTypeIdRequired)]
    public byte ResourceTypeID { get; set; }

    [JsonPropertyName("properties")]
    public List<UpdateCategoryPropertyRequest>? CategoryProperties { get; set; }
}

public class UpdateCategoryPropertyRequest
{
    public byte? PropertyID { get; set; }

    [Required]
    [StringLength(100)]
    public string PropertyName { get; set; } = null!;

    [Required]
    public byte DataTypeID { get; set; }
    public bool IsUnique { get; set; }
    public bool IsMandatory { get; set; }
}

public class CategoryDetailsResponse
{
    public byte CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public byte ResourceTypeID { get; set; }
    public string ResourceTypeName { get; set; } = string.Empty;
    public DateTime? CreatedDate { get; set; }
    public List<ResponseAssetProperty> CategoryProperties { get; set; } = new();
}

public class CategoryListItemResponse
{
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ResourceTypeName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
public class CategoryPagedResponse
{
    public List<CategoryListItemResponse> Categories { get; set; } = new();
    public int PageNo { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public string? Search { get; set; }
}

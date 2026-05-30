using System.ComponentModel.DataAnnotations;

namespace AssetManagement.AppService.DTOs
{
    public class CreateCategoryPropertyRequest
    {
        [Required]
        [StringLength(100)]
        public string PropertyName { get; set; } = null!;

        [Required]
        public byte DataTypeID { get; set; }
        public bool IsUnique { get; set; }
        public bool IsMandatory { get; set; }
    }
}
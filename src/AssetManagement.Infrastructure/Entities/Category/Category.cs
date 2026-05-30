using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssetManagement.Utility.Resource;

namespace AssetManagement.Infrastructure.Entities;

[Table("Category", Schema = "ref")]
public class Category
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public byte CategoryID { get; set; }

    [Required]
    [StringLength(50), MinLength(2)]
    [RegularExpression(@"^[a-zA-Z0-9\s\-]+$", ErrorMessage = CommonResource.CategoryNameInvalid)]
    public string CategoryName { get; set; } = null!;

    [Required]
    public byte ResourceTypeID { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedDate { get; set; }

    public DateTime? DeletedDate { get; set; }

    [ForeignKey("ResourceTypeID")]
    public virtual ResourceType ResourceType { get; set; } = null!;

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
    public virtual ICollection<AssetProperty> AssetProperties { get; set; } = new List<AssetProperty>();
}
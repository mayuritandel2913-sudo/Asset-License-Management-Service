using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities;

[Table("ResourceType",Schema ="ref")]
public class ResourceType
{
   [Key]
    public byte ResourceTypeID { get; set; } 
    [Required]
    [StringLength(50)]
    [Column("TypeName")]
    public string TypeName { get; set; } = null!;

    [Required]
    public DateTime CreatedDate { get; set; }

    public DateTime? DeletedDate { get; set; }


    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}

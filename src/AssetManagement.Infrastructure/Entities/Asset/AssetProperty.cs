using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities;

[Table("AssetProperty",Schema ="Asset")]
public class AssetProperty
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public byte PropertyID { get; set; } 
    [NotMapped]
    [StringLength(255)]
    public string? Value { get; set; }

    [Required]
    [StringLength(100)]
    public string PropertyName { get; set; } = null!;

    [Required]
    public byte CategoryID { get; set; }

    [Required]
    public byte DataTypeID { get; set; }
    public bool IsUnique { get; set; }
    public bool IsMandatory { get; set; }

    [Required]
    public int CreatedByID { get; set; }
    
    [Required]
    public DateTime CreatedDate { get; set; }

    public int? ModifiedByID { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? DeletedByID { get; set; }

    public DateTime? DeletedDate { get; set; }

    [ForeignKey("CategoryID")]
    public virtual Category Category { get; set; } = null!;

    [ForeignKey("DataTypeID")]
    public virtual DataType DataType { get; set; } = null!;

    public virtual ICollection<AssetPropertyValue> AssetPropertyValues { get; set; } = new List<AssetPropertyValue>();
}

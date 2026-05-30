using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 
namespace AssetManagement.Infrastructure.Entities;
 
[Table("AssetPropertyValue", Schema = "Asset")]
public class AssetPropertyValue
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AssetPropertyValueID { get; set; }
 
    [Required]
    public int AssetID { get; set; }
 
    [Required]
    public int CategoryID { get; set; }
 
    [Required]
    public byte PropertyID { get; set; }
 
    [Column("PropertyValue")]
    [StringLength(255)]
    public string? Value { get; set; }
   
    [Required]
    public int CreatedByID { get; set; }
   
    [Required]
    public DateTime CreatedDate { get; set; }
 
    public int? ModifiedByID { get; set; }
 
    public DateTime? ModifiedDate { get; set; }
 
    [ForeignKey("AssetID")]
    public virtual Asset Asset { get; set; } = null!;
 
    [ForeignKey("PropertyID")]
    public virtual AssetProperty AssetProperty { get; set; } = null!;
 
    [ForeignKey("CreatedByID")]
    public User CreatedBy { get; set; } = null!;
 
    [ForeignKey("ModifiedByID")]
    public User? ModifiedBy { get; set; }
}
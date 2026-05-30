using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities;

[Table("AssetStatus",Schema = "ref")]
public class AssetStatus
{
   [Key]
    public byte AssetStatusID { get; set; } 

    [Required]
    [StringLength(50)]
    public string AssetStatusName { get; set; } = null!; 
    
     public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}

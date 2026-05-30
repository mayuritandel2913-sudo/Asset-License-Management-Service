using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 
namespace AssetManagement.Infrastructure.Entities;
 
[Table("AssetHealthStatus",Schema = "ref")]
public class AssetHealthStatus
{
   [Key]
    public byte AssetHealthStatusID { get; set; }
 
    [Required]
    [StringLength(50)]
    public string AssetHealthStatusName { get; set; } = null!;
   
     public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}

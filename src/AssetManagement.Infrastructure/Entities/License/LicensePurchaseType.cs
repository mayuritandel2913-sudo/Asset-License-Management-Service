using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities.License;

[Table("LicensePurchaseType", Schema = "ref")]
public class LicensePurchaseType
{
    [Key]
    [Column("LicensePurchaseTypeID")]
    public byte LicensePurchaseTypeID { get; set; }

    [Required]
    [StringLength(50)]
    [Column("LicensePurchaseTypeName")]
    public string LicensePurchaseTypeName { get; set; } = null!;

    [Required]
    [Column("CreatedDate")]
    public DateTime CreatedDate { get; set; }   

    [Column("DeletedDate")]
    public DateTime? DeletedDate { get; set; }

}

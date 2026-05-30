using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities.License;

[Table("LicenseType", Schema = "ref")]
public class LicenseType
{
    [Key]
    [Column("LicenseTypeID")]
    public byte LicenseTypeID { get; set; }

    [Required]
    [StringLength(50)]
    [Column("LicenseTypeName")]
    public string LicenseTypeName { get; set; } = null!;

    [Required]
    [Column("CreatedDate")]
    public DateTime CreatedDate { get; set; }

    [Column("DeletedDate")]
    public DateTime? DeletedDate { get; set; }
}

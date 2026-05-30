using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities.License;

[Table("LicenseStatus", Schema = "ref")]
public class LicenseStatus
{
    [Key]
    [Column("LicenseStatusID")]
    public byte LicenseStatusID { get; set; }

    [Required]
    [StringLength(30)]
    [Column("LicenseStatusName")]
    public string LicenseStatusName { get; set; } = null!;

    [Required]
    [Column("CreatedDate")]
    public DateTime CreatedDate { get; set; }

    [Column("DeletedDate")]
    public DateTime? DeletedDate { get; set; }
}

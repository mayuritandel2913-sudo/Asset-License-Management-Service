using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities.License;

[Table("LicenseAuditLogDetail", Schema = "audit")]
public class LicenseAuditLogDetail
{
    [Key]
    [Column("LicenseAuditLogDetailID")]
    public int LicenseAuditLogDetailID { get; set; }

    [Required]
    [Column("LicenseAuditLogID")]
    public int LicenseAuditLogID { get; set; }

    [Required]
    [StringLength(100)]
    [Column("FieldName")]
    public string FieldName { get; set; } = null!;

    [Column("OldValue")]
    [StringLength(100)]
    public string? OldValue { get; set; }

    [Column("NewValue")]
    [StringLength(100)]
    public string? NewValue { get; set; }

    [ForeignKey("LicenseAuditLogID")]
    public virtual LicenseAuditLog LicenseAuditLog { get; set; } = null!;
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities.License;

[Table("LicenseAuditLog", Schema = "audit")]
public class LicenseAuditLog
{
    [Key]
    [Column("LicenseAuditLogID")]
    public int LicenseAuditLogID { get; set; }

    [Required]
    [Column("LicenseID")]
    public int LicenseID { get; set; }

    [Required]
    [Column("DateTimestamp")]
    public DateTime DateTimestamp { get; set; }

    [Required]
    [Column("ActionPerformedID")]
    public int ActionPerformedID { get; set; }

    [Required]
    [Column("PerformedByID")]
    public int PerformedByID { get; set; }

    [Column("EmployeeName")]
    [StringLength(255)]
    public string? EmployeeName { get; set; }

    [Column("EmployeeEmail")]
    [StringLength(255)]
    public string? EmployeeEmail { get; set; }

    [ForeignKey("LicenseID")]
    public virtual License License { get; set; } = null!;

    [ForeignKey("ActionPerformedID")]
    public virtual AssetManagement.Infrastructure.Entities.ActionType ActionType { get; set; } = null!;

    [ForeignKey("PerformedByID")]
    public virtual AssetManagement.Infrastructure.Entities.User PerformedBy { get; set; } = null!;

    public virtual ICollection<LicenseAuditLogDetail> Details { get; set; } = new List<LicenseAuditLogDetail>();
}

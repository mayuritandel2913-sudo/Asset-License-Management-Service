using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities;

[Table("AssetAuditLog", Schema = "audit")]
public class AssetAuditLog
{
    [Key]
    [Column("AssetAuditLogID")]
    public int AssetAuditLogID { get; set; }

    [Required]
    [Column("AssetID")]
    public int AssetID { get; set; }

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

    [ForeignKey("AssetID")]
    public virtual Asset Asset { get; set; } = null!;

    [ForeignKey("ActionPerformedID")]
    public virtual AssetManagement.Infrastructure.Entities.ActionType ActionType { get; set; } = null!;

    [ForeignKey("PerformedByID")]
    public virtual AssetManagement.Infrastructure.Entities.User PerformedBy { get; set; } = null!;

    public virtual ICollection<AssetAuditLogDetail> Details { get; set; } = new List<AssetAuditLogDetail>();
}

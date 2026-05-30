using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities;

[Table("AssetAuditLogDetail", Schema = "audit")]
public class AssetAuditLogDetail
{
    [Key]
    [Column("AssetAuditLogDetailID")]
    public int AssetAuditLogDetailID { get; set; }

    [Required]
    [Column("AssetAuditLogID")]
    public int AssetAuditLogID { get; set; }

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

    [ForeignKey("AssetAuditLogID")]
    public virtual AssetAuditLog AssetAuditLog { get; set; } = null!;
}

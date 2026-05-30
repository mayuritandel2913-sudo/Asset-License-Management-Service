using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities.License;

[Table("LicenseAssignment", Schema = "license")]
public class LicenseAssignment
{
    [Key]
    [Column("LicenseAssignmentID")]
    public int LicenseAssignmentID { get; set; }
    
    [Required]
    [Column("LicenseID")]
    public int LicenseID { get; set; }

    [Required]
    [Column("AssigneeID")]
    public int AssigneeID { get; set; }

    [Required]
    [Column("AssignmentDate")]
    public DateTime AssignmentDate { get; set; }

    [Required]
    [Column("AssignedBy")]
    [StringLength(50)]
    public string AssignedBy { get; set; } = null!;

    [Column("UnassignedDate")]
    public DateTime? UnassignedDate { get; set; }

    [Column("UnassignedBy")]
    [StringLength(50)]
    public string? UnassignedBy { get; set; }

    [Required]
    [Column("IsActive")]
    public bool IsActive { get; set; } = true;

    [Required]
    [Column("CreatedByID")]
    public int CreatedByID { get; set; }

    [Required]
    [Column("CreatedDate")]
    public DateTime CreatedDate { get; set; }

    [Column("ModifiedByID")]
    public int? ModifiedByID { get; set; }

    [Column("ModifiedDate")]
    public DateTime? ModifiedDate { get; set; }

    [ForeignKey("LicenseID")]
    public virtual License License { get; set; } = null!;

    [ForeignKey("AssigneeID")]
    public virtual User Assignee { get; set; } = null!;
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities;

[Table("AssignmentDetails", Schema = "Asset")]
public class AssignmentDetails
{
    [Key]
    [Column("AssignmentID")]
    public int AssignmentID { get; set; }

    [Required]
    [Column("UserID")]
    public int UserID { get; set; }

    [Required]
    [Column("AssetID")]
    public int AssetID { get; set; }

    [Required]
    [Column("AssignmentDate")]
    public DateTime AssignmentDate { get; set; }

    [Column("ExpectedReturnDate")]
    public DateTime? ExpectedReturnDate { get; set; }

    [Column("ActualReturnDate")]
    public DateTime? ActualReturnDate { get; set; }

    [Column("UnassignedByID")]
    public int? UnassignedByID { get; set; }

    [Column("UnassignedDate")]
    public DateTime? UnassignedDate { get; set; }

    [StringLength(500)]
    [Column("Remark")]
    public string? Remark { get; set; }

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

    [Column("DeletedByID")]
    public int? DeletedByID { get; set; }

    [Column("DeletedDate")]
    public DateTime? DeletedDate { get; set; }

    [ForeignKey("UserID")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("AssetID")]
    public Asset Asset { get; set; } = null!;

    [ForeignKey("CreatedByID")]
    public User CreatedBy { get; set; } = null!;

    [ForeignKey("ModifiedByID")]
    public User? ModifiedBy { get; set; }

    [ForeignKey("DeletedByID")]
    public User? DeletedBy { get; set; }
}
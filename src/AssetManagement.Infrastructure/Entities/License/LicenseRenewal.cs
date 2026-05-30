using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssetManagement.Infrastructure.Entities;

namespace AssetManagement.Infrastructure.Entities.License;

[Table("LicenseRenewal", Schema = "license")]
public class LicenseRenewal
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("LicenseRenewalID")]
    public int LicenseRenewalID { get; set; }
 
    [Required]
    [Column("LicenseID")]
    public int LicenseID { get; set; }
 
    [Required]
    [Column("LicenseRenewalDate")]
    public DateTime LicenseRenewalDate { get; set; }
 
    [Required]
    [Column("ExpiryDate")]
    public DateTime ExpiryDate { get; set; }
 
    [Column("UpdatedCost", TypeName = "decimal(18, 2)")]
    public decimal? UpdatedCost { get; set; }
 
    [Column("UpdatedTotalSeats")]
    public int? UpdatedTotalSeats { get; set; }
 
    [StringLength(100)]
    [Column("UpdatedLicenseKey")]
    public string? UpdatedLicenseKey { get; set; }
 
    [StringLength(100)]
    [Column("RenewalNotes")]
    public string? RenewalNotes { get; set; }
 
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
 
    [ForeignKey("CreatedByID")]
    public virtual User CreatedBy { get; set; } = null!;
 
    [ForeignKey("ModifiedByID")]
    public virtual User? ModifiedBy { get; set; }
}
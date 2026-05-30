using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities.License;

[Table("License", Schema = "license")]
public class License
{
    [Key]
    [Column("LicenseID")]
    public int LicenseID { get; set; }

    [Required]
    [StringLength(100)]
    [Column("LicenseName")]
    public string LicenseName { get; set; } = null!;

    [Required]
    [Column("LicenseTypeID")]
    public byte LicenseTypeID { get; set; }

    [Required]
    [Column("LicensePurchaseTypeID")]
    public byte LicensePurchaseTypeID { get; set; }

    [Required]
    [Column("TotalSeats")]
    public byte  TotalSeats { get; set; }

    [StringLength(50)]
    [Column("VendorName")]
    public string? VendorName { get; set; }

    [Required]
    [Column("PurchaseDate")]
    public DateTime PurchaseDate { get; set; }

    [Required]
    [Column("StartDate")]
    public DateTime StartDate { get; set; }

    [Column("ExpiryDate")]
    public DateTime? ExpiryDate { get; set; }

    [Required]
    [Column("LicenseStatusID")]
    public byte LicenseStatusID { get; set; }

    [StringLength(100)]
    [Column("LicenseKey")]
    public string? LicenseKey { get; set; }

    [Column("Cost", TypeName = "decimal(18,2)")]
    public decimal Cost { get; set; }

    [StringLength(300)]
    [Column("Description")]
    public string? Description { get; set; }

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

    [ForeignKey("LicenseTypeID")]
    public virtual LicenseType LicenseType { get; set; } = null!;

    [ForeignKey("LicensePurchaseTypeID")]
    public virtual LicensePurchaseType LicensePurchaseType { get; set; } = null!;

    [ForeignKey("LicenseStatusID")]
    public virtual LicenseStatus LicenseStatus { get; set; } = null!;

    [ForeignKey("CreatedByID")]
    public virtual User CreatedBy { get; set; } = null!;

    [ForeignKey("ModifiedByID")]
    public virtual User? ModifiedBy { get; set; }

    public virtual ICollection<LicenseReminder> LicenseReminders { get; set; } = new List<LicenseReminder>();
    public virtual ICollection<LicenseAssignment> LicenseAssignments { get; set; } = new List<LicenseAssignment>();
}

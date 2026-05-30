using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 
namespace AssetManagement.Infrastructure.Entities;
 
[Table("Asset", Schema = "Asset")]
public class Asset
{
    [Key]
    [Column("AssetID")]
    public int AssetID { get; set; }
 
    [NotMapped]
    public byte ResourceTypeID { get; set; }
 
    [Required]
    [Column("CategoryID")]
    public byte CategoryID { get; set; }
 
    [Required]
    [Column("StatusID")]
    public byte StatusID { get; set; } = 1;
 
    [Required]
    [StringLength(100)]
    [Column("AssetName")]
    public string AssetName { get; set; } = null!;
 
    [Required]
    [StringLength(100)]
    [Column("SerialNumber")]
    public string SerialNumber { get; set; } = null!;
 
    [Required]
    [Column("PurchaseDate")]
    public DateTime PurchaseDate { get; set; }
 
    [Column("AssetCost", TypeName = "decimal(18,2)")]
    public decimal? AssetCost { get; set; }
 
    [Column("UserID")]
    public int? UserID { get; set; }
 
    [Column("AssetHealthStatusID")]
    public byte? AssetHealthStatusID { get; set; } = 1;
 
    [StringLength(150)]
    [Column("VendorName")]
    public string? VendorName { get; set; }
 
    [StringLength(500)]
    [Column("Description")]
    public string? Description { get; set; }

    [StringLength(500)]
    [Column("HealthRemark")]
    public string? HealthRemark { get; set; }
 
    [Column("CreatedByID")]
    public int? CreatedByID { get; set; }
 
    [Column("CreatedDate")]
    public DateTime? CreatedDate { get; set; }
 
    [Column("ModifiedByID")]
    public int? ModifiedByID { get; set; }
 
    [Column("ModifiedDate")]
    public DateTime? ModifiedDate { get; set; }
 
    [Column("DeletedByID")]
    public int? DeletedByID { get; set; }
 
    [Column("DeletedDate")]
    public DateTime? DeletedDate { get; set; }
 
    [ForeignKey("CategoryID")]
    public Category Category { get; set; } = null!;
 
    [ForeignKey("StatusID")]
    public AssetStatus AssetStatus { get; set; } = null!;
 
    [ForeignKey("UserID")]
    public User? User { get; set; }
 
    [ForeignKey("AssetHealthStatusID")]
    public AssetHealthStatus? AssetHealthStatus { get; set; }
 
    [ForeignKey("CreatedByID")]
    public User CreatedBy { get; set; } = null!;
 
    [ForeignKey("ModifiedByID")]
    public User? ModifiedBy { get; set; }
 
    [ForeignKey("DeletedByID")]
    public User? DeletedBy { get; set; }
 
    public ICollection<AssignmentDetails> AssignmentDetails { get; set; } = new List<AssignmentDetails>();
    public virtual ICollection<AssetPropertyValue> PropertyValues { get; set; } = new List<AssetPropertyValue>();
   
}

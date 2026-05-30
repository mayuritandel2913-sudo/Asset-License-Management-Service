using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities;

[Table("User", Schema = "USER")]
public class User
{
    [Key]
    [Column("UserID")]
    public int UserID { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 2)]
    [Column("FirstName")]
    public string FirstName { get; set; } = null!;

    [Required]
    [Column("MiddleName")]
    public string? MiddleName { get; set; }
    [StringLength(50, MinimumLength = 2)]
       
    [Required]
    [Column("LastName")]
    public string LastName { get; set; } = null!;

    [Required]
    [StringLength(15, MinimumLength = 10)]
    [Column("Phone")]
    public string Phone { get; set; } = null!;

    [Required]
    [StringLength(80)]
    [Column("Email")]
    public string Email { get; set; } = null!;

    [Required]
    [Column("RoleID")]
    public byte RoleID { get; set; }

    [Column("IsActive")]
    public bool? IsActive { get; set; }

    [Column("DepartmentID")]
    public int? DepartmentID { get; set; }

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

    [ForeignKey("RoleID")]
    public Role Role { get; set; } = null!;

    [ForeignKey("DepartmentID")]
    public Department? Department { get; set; }

    [ForeignKey("CreatedByID")]
    public User CreatedBy { get; set; } = null!;

    [ForeignKey("ModifiedByID")]
    public User? ModifiedBy { get; set; }

    [ForeignKey("DeletedByID")]
    public User? DeletedBy { get; set; }
}
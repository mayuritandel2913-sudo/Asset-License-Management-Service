using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities;

[Table("Department", Schema = "ref")]
public class Department
{
    [Key]
    [Column("DepartmentID")]
    public int DepartmentID { get; set; }

    [Required]
    [StringLength(100)]
    [Column("DepartmentName")]
    public string DepartmentName { get; set; } = null!;

    [Required]
    [Column("CreatedDate")]
    public DateTime CreatedDate { get; set; }

    [Column("DeletedDate")]
    public DateTime? DeletedDate { get; set; }
}

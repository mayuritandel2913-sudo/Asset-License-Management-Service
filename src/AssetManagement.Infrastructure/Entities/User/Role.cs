using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities;

[Table("Role", Schema = "ref")]
public class Role
{
    [Key]
    [Column("RoleID")]
    public byte RoleID { get; set; }

    [Required]
    [StringLength(50)]
    [Column("RoleName")]
    public string RoleName { get; set; } = null!;

    [Required]
    [Column("CreatedDate")]
    public DateTime CreatedDate { get; set; }

    [Column("DeletedDate")]
    public DateTime? DeletedDate { get; set; }

    
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}

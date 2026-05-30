using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities;

[Table("ActionType", Schema = "ref")]
public class ActionType
{
    [Key]
    [Column("ActionID")]
    public int ActionID { get; set; }

    [Required]
    [StringLength(50)]
    [Column("ActionName")]
    public string ActionName { get; set; } = null!;


}

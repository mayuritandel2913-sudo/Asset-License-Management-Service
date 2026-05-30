using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities.License;

[Table("ReminderConfig", Schema = "ref")]
public class ReminderConfig
{
    [Key]
    [Column("ReminderConfigID")]
    public byte ReminderConfigID { get; set; }

    [Required]
    [Column("DaysBeforeExpiry")]
    public int DaysBeforeExpiry { get; set; }

    [Required]
    [Column("CreatedDate")]
    public DateTime CreatedDate { get; set; }

    [Column("DeletedDate")]
    public DateTime? DeletedDate { get; set; }
}

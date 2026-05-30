using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities.License;

[Table("LicenseReminder", Schema = "license")]
public class LicenseReminder
{
    [Key]
    [Column("LicenseReminderID")]
    public int LicenseReminderID { get; set; }

    [Required]
    [Column("LicenseID")]
    public int LicenseID { get; set; }

    [Required]
    [Column("ReminderConfigID")]
    public byte ReminderConfigID { get; set; }

    [Required]
    [Column("CreatedDate")]
    public DateTime CreatedDate { get; set; }

    [Required]
    [Column("CreatedByID")]
    public int CreatedByID { get; set; }

    [Column("ModifiedDate")]
    public DateTime? ModifiedDate { get; set; }

    [Column("ModifiedByID")]
    public int? ModifiedByID { get; set; }

    [Column("DeletedByID")]
    public int? DeletedByID { get; set; }

    [Column("DeletedDate")]
    public DateTime? DeletedDate { get; set; }

    [ForeignKey("LicenseID")]
    public virtual License License { get; set; } = null!;

    [ForeignKey("ReminderConfigID")]
    public virtual ReminderConfig ReminderConfig { get; set; } = null!;
}

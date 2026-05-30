using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssetManagement.Infrastructure.Entities.License;

namespace AssetManagement.Infrastructure.Entities.Notification
{
    [Table("Notification", Schema = "Notification")]
    public class Notification
    {
        [Key]
        [Column("NotificationID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationId { get; set; }
        
        [Column("NotificationStatusID")]
        public byte NotificationStatusId { get; set; }
        
        [Column("NotificationTypeID")]
        public byte NotificationTypeId { get; set; }
        
        [Column("LicenseID")]
        public int LicenseId { get; set; }
        
        [Column("EmployeeID")]
        public int EmployeeId { get; set; }
        
        [Column("CreatedByID")]
        public int CreatedByID { get; set; }
        
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

        [ForeignKey("NotificationStatusId")]
        public virtual NotificationStatus? NotificationStatus { get; set; }

        [ForeignKey("NotificationTypeId")]
        public virtual NotificationType? NotificationType { get; set; }

        [ForeignKey("LicenseId")]
        public virtual License.License? License { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual User? Employee { get; set; }
    }
}





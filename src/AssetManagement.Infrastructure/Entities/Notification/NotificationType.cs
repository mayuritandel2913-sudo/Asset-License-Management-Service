using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities.Notification
{
    [Table("NotificationType", Schema = "ref")]
    public class NotificationType
    {
        [Key]
        [Column("NotificationTypeID")]
        public byte NotificationTypeID { get; set; }
        
        [Required]
        [MaxLength(50)]
        [Column("NotificationTypeName")]
        public string NotificationTypeName { get; set; } = null!;
        
        [Column("CreatedDate")]
        public DateTime? CreatedDate { get; set; }
        
        [Column("DeletedDate")]
        public DateTime? DeletedDate { get; set; }
    }
}

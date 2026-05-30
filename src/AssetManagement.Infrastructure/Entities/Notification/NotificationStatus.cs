using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities.Notification
{
    [Table("NotificationStatus", Schema = "ref")]
    public class NotificationStatus
    {
        [Key]
        [Column("NotificationStatusID")]
        public byte NotificationStatusID { get; set; }
        
        [Required]
        [MaxLength(30)]
        [Column("NotificationStatusName")]
        public string NotificationStatusName { get; set; } = null!;
        
        [Column("CreatedDate")]
        public DateTime? CreatedDate { get; set; }
        
        [Column("DeletedDate")]
        public DateTime? DeletedDate { get; set; }
    }
}

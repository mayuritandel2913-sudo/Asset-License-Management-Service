using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Infrastructure.Entities;

[Table("DataType", Schema = "ref")]
public class DataType
{
    [Key]
    [Column("DataTypeID")]
    public byte DataTypeID { get; set; }

    [Required]
    [StringLength(100)]
    [Column("DataTypeName")]
    public string DataTypeName { get; set; } = null!;

    [Required]
    [Column("CreatedDate")]
    public DateTime CreatedDate { get; set; }

    [Column("DeletedDate")]
    public DateTime? DeletedDate { get; set; }
     public virtual ICollection<AssetProperty> AssetProperties { get; set; } = new List<AssetProperty>();
}
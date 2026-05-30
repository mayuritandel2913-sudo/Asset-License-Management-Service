namespace AssetManagement.AppService.DTOs;

public class ResponseAssetProperty
{
    public byte PropertyId { get; set; }
    public string PropertyName { get; set; } = null!;
    public byte DataTypeID { get; set; }
    public string DataType { get; set; } = string.Empty;
    public bool IsUnique { get; set; }
    public bool IsMandatory { get; set; }
    public string? PropertyValue { get; set; }
}

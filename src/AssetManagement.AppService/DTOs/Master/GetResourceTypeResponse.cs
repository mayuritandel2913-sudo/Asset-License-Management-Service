namespace AssetManagement.AppService.DTOs
{
  
    public class GetResourceTypeResponse
    {
        public byte ResourceTypeID { get; set; }
        public string TypeName { get; set; } = string.Empty;
    }
}
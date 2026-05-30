namespace AssetManagement.AppService.DTOs
{
      public class CategoryResponse
    {
        public byte CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public byte ResourceTypeID { get; set; }
        public string ResourceTypeName { get; set; } = string.Empty;
    }
}
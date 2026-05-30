namespace AssetManagement.Utility.Responses
{
    public class ErrorResponse
    {
        /// <summary>
        /// AuthServiceorName    : Mayuri Tandel
        /// Response Name   : ErrorResponse
        /// Description   : API error response model
        /// Creation-Date : 26th March 2026
        /// </summary>
        public string? Data { get; set; }
        public int StatusCode { get; set; }
        public string? Message { get; set; }
    }
}
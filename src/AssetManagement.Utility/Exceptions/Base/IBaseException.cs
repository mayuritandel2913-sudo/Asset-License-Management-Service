namespace AssetManagement.Utility.Exceptions.Base
{
    /// <summary>
    /// AuthServiceorName     : Mayuri Tandel
    /// Interface Name : IBaseException
    /// Description    : The base for creating any exception
    /// Creation-Date  : 26th March 2026
    /// </summary>
    public interface IBaseException
    {
        int StatusCode { get; }
        string ErrorMessage { get; }
    }
}
using AssetManagement.Utility.Exceptions.Base;

namespace AssetManagement.Utility.Exceptions;

/// <summary>
/// AuthServiceorName    : Mayuri Tandel
/// Method Name   : BaseException
/// Description   : custom base class for application exception
/// Creation-Date : 26th March 2026
/// </summary>
public class BaseException : Exception,IBaseException

{
    public int StatusCode { get; }
    public string ErrorMessage { get; }
    protected BaseException(int statusCode, string errorMessage) : base(errorMessage)
    {
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
    }
}

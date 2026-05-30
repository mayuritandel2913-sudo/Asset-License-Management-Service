using AssetManagement.Utility.Resource;

namespace AssetManagement.Utility.Exceptions;

public class InternalServerException : BaseException
{
    /// <summary>
    /// AuthServiceorName    : Mayuri Tandel
    /// Method Name   : InternalServerException
    /// Description   : Handel Unexpected server error
    /// Creation-Date : 26th March 2026
    /// </summary>
    public InternalServerException(string? message = null) :
        base(500, message ?? CommonResource.InternalServerError)
    {
    }
}
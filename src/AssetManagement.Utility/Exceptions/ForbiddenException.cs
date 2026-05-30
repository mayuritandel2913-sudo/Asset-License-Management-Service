using AssetManagement.Utility.Resource;

namespace AssetManagement.Utility.Exceptions;

public class ForbiddenException : BaseException
{
    /// <summary>
    /// AuthServiceorName    : Mayuri Tandel
    /// Method Name   : ForbiddenException
    /// Description   : Handle denied for the requested resources
    /// Creation-Date : 26th March 2026
    /// </summary>
    public ForbiddenException(string? message = null) :
        base(403, message ?? CommonResource.Forbidden)
    {
    }
}

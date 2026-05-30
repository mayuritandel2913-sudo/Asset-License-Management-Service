using AssetManagement.Utility.Resource;

namespace AssetManagement.Utility.Exceptions;

public class UnAuthServiceorizedException : BaseException
{
    /// <summary>
    /// AuthServiceorName    : Mayuri Tandel
    /// Method Name   : UnAuthServiceorizedException
    /// Description   : AuthServiceentication failure
    /// Creation-Date : 26th March 2026
    /// </summary>
    public UnAuthServiceorizedException(string? message = null) :
       base(401, message ?? CommonResource.UnAuthServiceorized)
    {
    }
}

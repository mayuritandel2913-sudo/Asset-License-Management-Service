using AssetManagement.Utility.Resource;

namespace AssetManagement.Utility.Exceptions;

public class BadRequestException : BaseException
{
    /// <summary>
    /// AuthServiceorName    : Mayuri Tandel
    /// Method Name   : BadRequestException
    /// Description   : Handle Invalid request data
    /// Creation-Date : 26th March 2026
    /// </summary>
    public BadRequestException(string? message = null) :
        base(400, message ?? CommonResource.BadRequest)
    {
    }
}

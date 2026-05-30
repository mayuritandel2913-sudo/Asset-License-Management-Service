using AssetManagement.Utility.Resource;

namespace AssetManagement.Utility.Exceptions;

public class NotFoundException : BaseException
{
    /// <summary>
    /// AuthServiceorName    : Mayuri Tandel
    /// Method Name   : NotFoundException
    /// Description   : Resource not found
    /// Creation-Date : 26th March 2026
    /// </summary>
    public NotFoundException(string? message = null) :
        base(404, message ?? CommonResource.NotFound)
    {
    }
}

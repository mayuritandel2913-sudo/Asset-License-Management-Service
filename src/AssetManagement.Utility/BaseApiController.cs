using Microsoft.AspNetCore.Mvc;
using AssetManagement.Utility.Resource;

namespace AssetManagement.Utility;

public class BaseApiController : ControllerBase
{
    protected new IActionResult Ok()
    {
        return base.Ok(Envelope.Ok(null, CommonResource.Success, 200));
    }

    protected IActionResult Ok<T>(T result, string? message = null)
    {
        return base.Ok(Envelope<T>.Ok(result, message ?? CommonResource.Success, 200));
    }

    protected IActionResult Error(string errorMessage)
    {
        return BadRequest(Envelope.Error(errorMessage, 400));
    }
}

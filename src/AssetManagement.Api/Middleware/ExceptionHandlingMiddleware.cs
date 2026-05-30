using System.Net;
using System.Text.Json;
using AssetManagement.Utility.Exceptions.Base;
using AssetManagement.Utility.Resource;
using AssetManagement.Utility.Responses;

namespace AssetManagement.API.Middleware;

/// <summary>
/// AuthorName      : Mayuri Tandel
/// MiddleWare Name : ExceptionHandlingMiddleware
/// Description     : Global exception handling middleware
/// Creation-Date   : 26th March 2026
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        ErrorResponse response;

        if (exception is IBaseException baseException)
        {
            context.Response.StatusCode = baseException.StatusCode;
            response = new ErrorResponse
            {
                StatusCode = baseException.StatusCode,
                Message = baseException.ErrorMessage,
            };
        }
        else
        {
            _logger.LogError(exception, "Unhandled exception | Path: {Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response = new ErrorResponse
            {
                StatusCode = 500,
                Message = CommonResource.InternalServerError,
            };
        }

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace YourGamesList.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (UnauthorizedAccessException exception)
        {
            _logger.LogInformation(exception, "Unauthorized access exception during executing {RequestPath}", context.Request.Path.Value);
            var response = context.Response;
            response.StatusCode = StatusCodes.Status401Unauthorized;
            await response.WriteAsync(string.Empty);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception during executing {RequestPath}", context.Request.Path.Value);
            var response = context.Response;
            response.StatusCode = StatusCodes.Status500InternalServerError;
            await response.WriteAsync(string.Empty);
            return;
        }
    }
}
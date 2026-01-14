using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Model;
using YourGamesList.Api.ModelBinders;

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
        catch (ModelBindingException exception)
        {
            _logger.LogError(exception,
                $"Exception during model binding of '{exception.ModelName}' of type '{exception.ModelType}'. Detailed error message: '{exception.ErrorDescription}'.");
            var response = context.Response;

            if (exception.ModelType == typeof(JwtUserInformation))
            {
                _logger.LogInformation($"'{typeof(JwtUserInformation)}' binding exception. Will return '{(int) HttpStatusCode.Unauthorized}' status code.");
                response.StatusCode = (int) HttpStatusCode.Unauthorized;
            }
            else
            {
                response.StatusCode = (int) HttpStatusCode.InternalServerError;
            }

            await response.WriteAsync(string.Empty);
            return;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Unhandled exception during executing '{context.Request.Path.Value}'");
            var response = context.Response;
            response.StatusCode = (int) HttpStatusCode.InternalServerError;
            await response.WriteAsync(string.Empty);
            return;
        }
    }
}
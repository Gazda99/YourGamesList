using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Controllers;
using YourGamesList.Api.Exceptions;
using YourGamesList.Api.Log;
using YourGamesList.Common.Http;

namespace YourGamesList.Api.Filters;

public class YglExceptionFilterAttribute : IExceptionFilter, IActionFilter
{
    private InputArguments? _inputArguments = null;
    private readonly ILogger<YglExceptionFilterAttribute> _logger;

    public YglExceptionFilterAttribute(ILogger<YglExceptionFilterAttribute> logger)
    {
        _logger = logger;
    }


    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.Controller is not YglControllerBase controller)
            throw new ArgumentException(nameof(context.Controller));

        _inputArguments = controller.InputArguments;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public void OnException(ExceptionContext context)
    {
        using var l = _logger.WithCorrelationId(_inputArguments?.CorrelationId);
        _logger.LogWarning(context.Exception, "Exception filter fired.");

        context.HttpContext.Response.Headers.AddCorrelationId(_inputArguments?.CorrelationId);

        if (context.Exception is InvalidRequestException invalidRequestException)
        {
            context.Result = new BadRequestObjectResult(invalidRequestException.Message);
        }

        else
        {
            context.Result = HandleUnknownError();
        }
    }

    private static ContentResult HandleUnknownError()
    {
        return new ContentResult()
        {
            StatusCode = 500,
            Content =
                $"Unknown error happened. Please contact admin with \"{HeaderDefs.HeaderCorrelationIdName}\" found in response header.",
            ContentType = ContentTypes.TextPlain
        };
    }
}
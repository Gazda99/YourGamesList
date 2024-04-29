using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Exceptions;
using YourGamesList.Common.Http;

namespace YourGamesList.Api.Filters;

public class YglExceptionFilterAttribute : IExceptionFilter, IActionFilter
{
    private readonly ILogger<YglExceptionFilterAttribute> _logger;

    public YglExceptionFilterAttribute(ILogger<YglExceptionFilterAttribute> logger)
    {
        _logger = logger;
    }


    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogWarning(context.Exception, "Exception filter fired.");

        //TODO: Is this exception still being thrown by YGL app?
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
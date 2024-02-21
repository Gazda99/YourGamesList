using Microsoft.AspNetCore.Mvc.Filters;
using YourGamesList.Api.Controllers;
using YourGamesList.Api.Log;
using YourGamesList.Common.Http;

namespace YourGamesList.Api.Filters;

public class InputsFilterAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.Controller is not YglControllerBase controller)
            throw new ArgumentException(nameof(context.Controller));

        var inputArguments = new InputArguments();
        inputArguments.CorrelationId = LogScopeExtensions.CreateCorrelationId();

        controller.InputArguments = inputArguments;

        await next();
    }

    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Controller is not YglControllerBase controller)
            throw new ArgumentException(nameof(context.Controller));

        var inputArguments = controller.InputArguments;
        context.HttpContext.Response.Headers.AddCorrelationId(inputArguments.CorrelationId);

        await next();
    }
}
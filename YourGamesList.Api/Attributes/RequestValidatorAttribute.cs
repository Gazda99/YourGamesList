using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.ControllerModelValidators;

namespace YourGamesList.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequestValidatorAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return ActivatorUtilities.CreateInstance<ValidateRequestFilter>(serviceProvider);
    }
}

public class ValidateRequestFilter : IAsyncActionFilter
{
    private readonly ILogger<ValidateRequestFilter> _logger;
    private readonly IValidationFailedResultFactory _resultFactory;

    public ValidateRequestFilter(ILogger<ValidateRequestFilter> logger, IValidationFailedResultFactory resultFactory)
    {
        _logger = logger;
        _resultFactory = resultFactory;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values.Where(v => v != null))
        {
            var argumentType = argument!.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);

            if (context.HttpContext.RequestServices.GetService(validatorType) is IValidator validator)
            {
                var validationContext = new ValidationContext<object>(argument);
                var validationResult = await validator.ValidateAsync(validationContext);

                if (!validationResult.IsValid)
                {
                    _logger.LogInformation("Validation of model {RequestType} failed.", argumentType);
                    context.Result = _resultFactory.CreateValidationFailedResult(validationResult);
                    return;
                }
                else
                {
                    _logger.LogInformation("Validation of model {RequestType} passed.", argumentType);
                }
            }
        }

        await next();
    }
}
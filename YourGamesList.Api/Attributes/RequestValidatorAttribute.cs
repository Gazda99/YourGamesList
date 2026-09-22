using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using YourGamesList.Api.Services.ControllerModelValidators;

namespace YourGamesList.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequestValidatorAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 1. Resolve your custom factory from the DI container
        var resultFactory = context.HttpContext.RequestServices.GetRequiredService<IValidationFailedResultFactory>();

        // 2. Loop through all arguments passed to the controller action
        foreach (var argument in context.ActionArguments.Values.Where(v => v != null))
        {
            var argumentType = argument!.GetType();

            // 3. Ask DI if there is a registered IValidator<T> for this argument type
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);

            if (context.HttpContext.RequestServices.GetService(validatorType) is IValidator validator)
            {
                // 4. Run the validation
                var validationContext = new ValidationContext<object>(argument);
                var validationResult = await validator.ValidateAsync(validationContext);

                // 5. If it fails, short-circuit the request and return the 400 Bad Request
                if (!validationResult.IsValid)
                {
                    context.Result = resultFactory.CreateValidationFailedResult(validationResult);
                    return;
                }
            }
        }

        // 6. If everything is valid (or no validators were found), continue to the controller
        await next();
    }
}
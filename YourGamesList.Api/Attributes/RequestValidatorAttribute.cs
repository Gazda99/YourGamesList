using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Model.Responses;
using YourGamesList.Api.Services.ControllerModelValidators;

namespace YourGamesList.Api.Attributes;

public class RequestValidatorAttribute<TRequest> : ActionFilterAttribute
{
    private readonly ILogger<RequestValidatorAttribute<TRequest>> _logger;
    private readonly IBaseRequestValidator _baseRequestValidator;
    private readonly IValidator<TRequest> _validator;
    private readonly string _argumentName;

    public RequestValidatorAttribute(
        ILogger<RequestValidatorAttribute<TRequest>> logger,
        IBaseRequestValidator baseRequestValidator,
        IValidator<TRequest> validator,
        string argumentName
    )
    {
        _logger = logger;
        _baseRequestValidator = baseRequestValidator;
        _validator = validator;
        _argumentName = argumentName;
    }

    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        if (!Validate(filterContext, _argumentName, out var validationFailedResult))
        {
            filterContext.Result = validationFailedResult;
        }
    }

    private bool Validate(
        ActionExecutingContext filterContext,
        string argumentName,
        out IActionResult? validationFailedResult)
    {
        const string missingArgumentsErrorMessage = "Missing required arguments.";

        if (!filterContext.ActionArguments.TryGetValue(argumentName, out var requestObject))
        {
            _logger.LogWarning($"'{argumentName}' argument is missing from request.");

            var errorResponse = new ErrorResponse()
            {
                Errors = [missingArgumentsErrorMessage]
            };

            validationFailedResult = new ObjectResult(errorResponse)
            {
                StatusCode = 400,
            };

            return false;
        }

        if (requestObject is not TRequest request)
        {
            _logger.LogWarning($"'{argumentName}' argument is null or not of type '{typeof(TRequest)}'.");

            var errorResponse = new ErrorResponse()
            {
                Errors = [missingArgumentsErrorMessage]
            };

            validationFailedResult = new ObjectResult(errorResponse)
            {
                StatusCode = 400,
            };

            return false;
        }

        return _baseRequestValidator.Validate(_validator, request, out validationFailedResult);
    }
}
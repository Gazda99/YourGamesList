using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace YourGamesList.Api.Services.ControllerModelValidators;

public interface IBaseRequestValidator
{
    bool Validate<TRequest>(
        IValidator<TRequest> requestValidator,
        TRequest request,
        [NotNullWhen(false)] out IActionResult? validationFailedResult
    );
}

public class BaseRequestValidator : IBaseRequestValidator
{
    private readonly ILogger<BaseRequestValidator> _logger;
    private readonly IValidationFailedResultFactory _validationFailedResultFactory;

    public BaseRequestValidator(ILogger<BaseRequestValidator> logger, IValidationFailedResultFactory validationFailedResultFactory)
    {
        _logger = logger;
        _validationFailedResultFactory = validationFailedResultFactory;
    }

    public bool Validate<TRequest>(
        IValidator<TRequest> requestValidator,
        TRequest request,
        [NotNullWhen(false)] out IActionResult? validationFailedResult
    )
    {
        var validationResult = requestValidator.Validate(request);
        if (validationResult.IsValid)
        {
            _logger.LogInformation("Validation of model {RequestType} passed.", typeof(TRequest));
            validationFailedResult = null;
            return true;
        }

        var errorList = validationResult.Errors.Select(x => x.ErrorMessage);
        _logger.LogWarning("Validation of model '{RequestType}' failed. Found {ValidationErrorCount} errors. Errors: {ErrorList}", typeof(TRequest), validationResult.Errors.Count, errorList);

        validationFailedResult = _validationFailedResultFactory.CreateValidationFailedResult(validationResult);
        return false;
    }
}
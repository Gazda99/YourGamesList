using System.Linq;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.Services.ControllerModelValidators;

public interface IValidationFailedResultFactory
{
    /// <summary>
    /// Creates a response for failed request validation.
    /// </summary>
    IActionResult CreateValidationFailedResult(ValidationResult? validationResult);
}

public class ValidationFailedResultFactory : IValidationFailedResultFactory
{
    private readonly ILogger<ValidationFailedResultFactory> _logger;

    public ValidationFailedResultFactory(ILogger<ValidationFailedResultFactory> logger)
    {
        _logger = logger;
    }

    public IActionResult CreateValidationFailedResult(ValidationResult? validationResult)
    {
        var errors = validationResult?.Errors ?? [];
        var errorResponse = new ErrorResponse()
        {
            Errors = errors.Select(x => new Error()
            {
                Code = x.ErrorCode,
                Description = x.ErrorMessage
            })
        };

        foreach (var validationFailure in errors)
        {
            _logger.LogInformation("Found request validation error for {PropertyName} with code {ErrorCode} and message {ErrorMessage}", validationFailure.PropertyName, validationFailure.ErrorCode, validationFailure.ErrorMessage );
        }
        
        return new ObjectResult(errorResponse)
        {
            StatusCode = 400,
        };
    }
}
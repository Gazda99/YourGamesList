using System.Linq;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.Services.ControllerModelValidators;

public interface IValidationFailedResultFactory
{
    IActionResult CreateValidationFailedResult(ValidationResult? validationResult);
}

public class ValidationFailedResultFactory : IValidationFailedResultFactory
{
    public IActionResult CreateValidationFailedResult(ValidationResult? validationResult)
    {
        var errors = validationResult?.Errors.Select(x => x.ErrorMessage) ?? [];

        var errorResponse = new ErrorResponse()
        {
            Errors = errors
        };

        return new ObjectResult(errorResponse)
        {
            StatusCode = 400,
        };
    }
}
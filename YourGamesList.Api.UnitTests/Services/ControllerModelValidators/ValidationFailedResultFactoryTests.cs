using System.Linq;
using AutoFixture;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Services.ControllerModelValidators;
using YourGamesList.Contracts.Responses;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.ControllerModelValidators;

public class ValidationFailedResultFactoryTests
{
    private IFixture _fixture;
    private ILogger<ValidationFailedResultFactory> _logger;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<ValidationFailedResultFactory>>();
    }

    [Test]
    public void CreateValidationFailedResult_ReturnsAllErrors()
    {
        //ARRANGE
        var validationResult = _fixture.Create<ValidationResult>();
        var expectedErrors = validationResult.Errors.Select(x => x.ErrorCode);
        var validationFailedResultFactory = new ValidationFailedResultFactory(_logger);

        //ACT
        var res = validationFailedResultFactory.CreateValidationFailedResult(validationResult);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = res as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(400));
        Assert.That(objectResult?.Value, Is.TypeOf<ErrorResponse>());
        var errorResponse = objectResult?.Value as ErrorResponse;
        Assert.That(errorResponse.Errors.Select(x=>x.Code), Is.EquivalentTo(expectedErrors));

        foreach (var error in validationResult.Errors)
        {
            _logger.ReceivedLog(
                LogLevel.Information,
                $"Found request validation error for {error.PropertyName} with code {error.ErrorCode} and message {error.ErrorMessage}");
        }
    }
}
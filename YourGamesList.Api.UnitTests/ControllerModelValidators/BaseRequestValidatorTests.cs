using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.ControllerModelValidators;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.ControllerModelValidators;

public class BaseRequestValidatorTests
{
    private IFixture _fixture;
    private ILogger<BaseRequestValidator> _logger;
    private IValidationFailedResultFactory _validationFailedResultFactory;
    private BaseRequestValidator _baseRequestValidator;

    private IValidator<object> _requestValidator;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<BaseRequestValidator>>();
        _validationFailedResultFactory = Substitute.For<IValidationFailedResultFactory>();
        _baseRequestValidator = new BaseRequestValidator(_logger, _validationFailedResultFactory);

        _requestValidator = Substitute.For<IValidator<object>>();
    }

    [Test]
    public void Validate_ReturnsTrue_OnValidModel()
    {
        //ARRANGE
        var request = _fixture.Create<object>();
        var successValidationResult = new ValidationResult();
        _requestValidator.Validate(request).Returns(successValidationResult);

        //ACT
        var res = _baseRequestValidator.Validate(_requestValidator, request, out var validationFailedResult);

        //ASSERT
        Assert.That(res, Is.True);
        Assert.That(validationFailedResult, Is.Null);
        _logger.ReceivedLog(LogLevel.Information, $"Validation of model '{typeof(object)}' passed.");
    }

    [Test]
    public void Validate_ReturnsFalse_OnInvalidModel()
    {
        //ARRANGE
        var request = _fixture.Create<object>();
        var validationFailures = _fixture.CreateMany<ValidationFailure>();
        var failureValidationResult = new ValidationResult(validationFailures);
        var expectedActionResult = _fixture.Create<ObjectResult>();
        _requestValidator.Validate(request).Returns(failureValidationResult);
        _validationFailedResultFactory.CreateValidationFailedResult(failureValidationResult).Returns(expectedActionResult);

        //ACT
        var res = _baseRequestValidator.Validate(_requestValidator, request, out var validationFailedResult);

        //ASSERT
        Assert.That(res, Is.False);
        Assert.That(validationFailedResult, Is.Not.Null);
        Assert.That(validationFailedResult, Is.EqualTo(expectedActionResult));

        var errorList = failureValidationResult.Errors.Select(x => x.ErrorMessage);
        var kw = new List<string>(errorList)
        {
            $"Validation of model '{typeof(object)}' failed. Found {failureValidationResult.Errors.Count} errors. Errors:"
        };
        _logger.ReceivedLog(LogLevel.Warning, kw.ToArray());
    }
}
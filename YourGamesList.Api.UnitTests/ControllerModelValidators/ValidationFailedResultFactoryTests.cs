using System.Linq;
using AutoFixture;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Services.ControllerModelValidators;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.UnitTests.ControllerModelValidators;

public class ValidationFailedResultFactoryTests
{
    private IFixture _fixture;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void CreateValidationFailedResult_ReturnsAllErrors()
    {
        //ARRANGE
        var validationResult = _fixture.Create<ValidationResult>();
        var expectedErrors = validationResult?.Errors.Select(x => x.ErrorMessage);
        var validationFailedResultFactory = new ValidationFailedResultFactory();

        //ACT
        var res = validationFailedResultFactory.CreateValidationFailedResult(validationResult);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = res as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(400));
        Assert.That(objectResult?.Value, Is.TypeOf<ErrorResponse>());
        var errorResponse = objectResult?.Value as ErrorResponse;
        Assert.That(errorResponse.Errors, Is.EquivalentTo(expectedErrors));
    }
}
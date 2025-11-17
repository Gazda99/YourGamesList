using System.Collections.Generic;
using AutoFixture;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Responses;
using YourGamesList.Api.Services.ControllerModelValidators;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Attributes;

public class RequestValidatorAttributeTests
{
    private IFixture _fixture;
    private ILogger<RequestValidatorAttribute<TestClass>> _logger;
    private IBaseRequestValidator _baseRequestValidator;
    private IValidator<TestClass> _validator;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<RequestValidatorAttribute<TestClass>>>();
        _baseRequestValidator = Substitute.For<IBaseRequestValidator>();
        _validator = Substitute.For<IValidator<TestClass>>();
    }

    [Test]
    public void Validate_OnMissingArgument_ReturnsCorrectErrorResponse()
    {
        //ARRANGE
        var argumentName = _fixture.Create<string>();

        var filterContext = PrepareActionExecutingContext(
            // Won't be able to TryGetValue argumentName
            actionArguments: new Dictionary<string, object>()
        );
        var requestValidatorAttribute = new RequestValidatorAttribute<TestClass>(_logger, _baseRequestValidator, _validator, argumentName);

        //ACT
        requestValidatorAttribute.OnActionExecuting(filterContext);

        //ASSERT

        Assert.That(filterContext.Result, Is.TypeOf<ObjectResult>());
        var objectResult = filterContext.Result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult.StatusCode, Is.EqualTo(400));
        Assert.That(objectResult.Value, Is.TypeOf<ErrorResponse>());
        var errorResponse = objectResult.Value as ErrorResponse;
        Assert.That(errorResponse.Errors, Contains.Item("Missing required arguments."));
        _logger.ReceivedLog(LogLevel.Warning, $"'{argumentName}' argument is missing from request.");
    }

    [Test]
    public void Validate_OnWrongArgumentType_ReturnsCorrectErrorResponse()
    {
        //ARRANGE
        var argumentName = _fixture.Create<string>();

        var filterContext = PrepareActionExecutingContext(
            // Wrong type of argument TestClass != OtherTestClass
            actionArguments: new Dictionary<string, object>()
            {
                { argumentName, _fixture.Create<OtherTestClass>() }
            }
        );
        var requestValidatorAttribute = new RequestValidatorAttribute<TestClass>(_logger, _baseRequestValidator, _validator, argumentName);

        //ACT
        requestValidatorAttribute.OnActionExecuting(filterContext);

        //ASSERT

        Assert.That(filterContext.Result, Is.TypeOf<ObjectResult>());
        var objectResult = filterContext.Result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult.StatusCode, Is.EqualTo(400));
        Assert.That(objectResult.Value, Is.TypeOf<ErrorResponse>());
        var errorResponse = objectResult.Value as ErrorResponse;
        Assert.That(errorResponse.Errors, Contains.Item("Missing required arguments."));
        _logger.ReceivedLog(LogLevel.Warning, $"'{argumentName}' argument is null or not of type '{typeof(TestClass)}'.");
    }

    [Test]
    public void Validate_OnValidateTrue_DoesNothing()
    {
        //ARRANGE
        var argumentName = _fixture.Create<string>();
        var request = _fixture.Create<TestClass>();
        var filterContext = PrepareActionExecutingContext(
            actionArguments: new Dictionary<string, object>()
            {
                { argumentName, request }
            }
        );
        _baseRequestValidator.Validate(_validator, request, out _).Returns(true);

        var requestValidatorAttribute = new RequestValidatorAttribute<TestClass>(_logger, _baseRequestValidator, _validator, argumentName);

        //ACT
        requestValidatorAttribute.OnActionExecuting(filterContext);

        //ASSERT
        Assert.That(filterContext.Result, Is.Null);
    }

    [Test]
    public void Validate_OnValidateFalse_ReturnsValidationResult()
    {
        //ARRANGE
        var argumentName = _fixture.Create<string>();
        var request = _fixture.Create<TestClass>();
        var filterContext = PrepareActionExecutingContext(
            actionArguments: new Dictionary<string, object>()
            {
                { argumentName, request }
            }
        );

        var validationFailedResult = _fixture.Create<ObjectResult>();

        _baseRequestValidator.Validate(_validator, request, out Arg.Any<IActionResult?>()).Returns(x =>
        {
            x[2] = validationFailedResult;
            return false;
        });

        var requestValidatorAttribute = new RequestValidatorAttribute<TestClass>(_logger, _baseRequestValidator, _validator, argumentName);

        //ACT
        requestValidatorAttribute.OnActionExecuting(filterContext);

        //ASSERT
        Assert.That(filterContext.Result, Is.EqualTo(validationFailedResult));
    }

    private ActionExecutingContext PrepareActionExecutingContext(
        Dictionary<string, object>? actionArguments = null
    )
    {
        var httpContext = new DefaultHttpContext();
        var routeData = _fixture.Create<RouteData>();
        var actionDescriptor = new ActionDescriptor();

        var context = new ActionExecutingContext(
            actionContext: new ActionContext(
                httpContext,
                routeData,
                actionDescriptor
            ),
            filters: new List<IFilterMetadata>(),
            actionArguments: actionArguments ?? new Dictionary<string, object>(),
            controller: new object()
        );

        return context;
    }

    public class TestClass
    {
    }

    public class OtherTestClass
    {
    }
}
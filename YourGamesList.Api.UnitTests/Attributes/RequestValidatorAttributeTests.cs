using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Services.ControllerModelValidators;

namespace YourGamesList.Api.UnitTests.Attributes;

[TestFixture]
public class RequestValidatorAttributeTests
{
    private IFixture _fixture;
    private IServiceProvider _serviceProvider;
    private IValidationFailedResultFactory _resultFactory;
    private IValidator _validator;
    private RequestValidatorAttribute _attribute;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _serviceProvider = Substitute.For<IServiceProvider>();
        _resultFactory = Substitute.For<IValidationFailedResultFactory>();
        _validator = Substitute.For<IValidator>();

        _serviceProvider
            .GetService(typeof(IValidationFailedResultFactory))
            .Returns(_resultFactory);

        _attribute = new RequestValidatorAttribute();
    }

    [Test]
    public async Task OnActionExecutionAsync_WhenNoValidatorRegisteredForArgument_CallsNextAndDoesNotSetResult()
    {
        //ARRANGE
        var argumentName = _fixture.Create<string>();
        var requestObject = new TestClass();

        var filterContext = PrepareActionExecutingContext(
            actionArguments: new Dictionary<string, object>
            {
                { argumentName, requestObject }
            }
        );

        _serviceProvider.GetService(typeof(IValidator<TestClass>)).Returns(null);

        var nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        };

        //ACT
        await _attribute.OnActionExecutionAsync(filterContext, next);

        //ASSERT
        Assert.That(nextCalled, Is.True);
        Assert.That(filterContext.Result, Is.Null);
    }

    [Test]
    public async Task OnActionExecutionAsync_WhenValidationPasses_CallsNextAndDoesNotSetResult()
    {
        //ARRANGE
        var argumentName = _fixture.Create<string>();
        var requestObject = new TestClass();

        var filterContext = PrepareActionExecutingContext(
            actionArguments: new Dictionary<string, object>
            {
                { argumentName, requestObject }
            }
        );
        
        var validResult = new ValidationResult();
        _validator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(validResult));
        
        _serviceProvider.GetService(typeof(IValidator<TestClass>)).Returns(_validator);

        var nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        };

        //ACT
        await _attribute.OnActionExecutionAsync(filterContext, next);

        //ASSERT
        Assert.That(nextCalled, Is.True);
        Assert.That(filterContext.Result, Is.Null);
    }

    [Test]
    public async Task OnActionExecutionAsync_WhenValidationFails_ShortCircuitsAndSetsResult()
    {
        //ARRANGE
        var argumentName = _fixture.Create<string>();
        var requestObject = new TestClass();

        var filterContext = PrepareActionExecutingContext(
            actionArguments: new Dictionary<string, object>
            {
                { argumentName, requestObject }
            }
        );
        
        var invalidResult = new ValidationResult([new ValidationFailure("Property", "Error")]);
        _validator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(invalidResult));

        _serviceProvider.GetService(typeof(IValidator<TestClass>)).Returns(_validator);

        var expectedFailedResult = new BadRequestObjectResult("Validation Failed");
        _resultFactory.CreateValidationFailedResult(invalidResult).Returns(expectedFailedResult);

        var nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        };

        //ACT
        await _attribute.OnActionExecutionAsync(filterContext, next);

        //ASSERT
        Assert.That(nextCalled, Is.False, "The 'next' delegate should not be called when validation fails.");
        Assert.That(filterContext.Result, Is.EqualTo(expectedFailedResult));
    }

    [Test]
    public async Task OnActionExecutionAsync_WithMultipleArguments_ValidatesSuccessfully()
    {
        //ARRANGE
        var filterContext = PrepareActionExecutingContext(
            actionArguments: new Dictionary<string, object>
            {
                { "id", Guid.NewGuid() },
                { "request", new TestClass() }
            }
        );

        _serviceProvider.GetService(typeof(IValidator<Guid>)).Returns(null);

        var validResult = new ValidationResult();
        _validator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(validResult));

        _serviceProvider.GetService(typeof(IValidator<TestClass>)).Returns(_validator);

        var nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        };

        //ACT
        await _attribute.OnActionExecutionAsync(filterContext, next);

        //ASSERT
        Assert.That(nextCalled, Is.True);
        Assert.That(filterContext.Result, Is.Null);
        
        await _validator.Received(1).ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>());
    }

    private ActionExecutingContext PrepareActionExecutingContext(
        Dictionary<string, object>? actionArguments = null
    )
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = _serviceProvider
        };

        var routeData = new RouteData();
        var actionDescriptor = new ActionDescriptor();

        var context = new ActionExecutingContext(
            actionContext: new ActionContext(
                httpContext,
                routeData,
                actionDescriptor
            ),
            filters: new List<IFilterMetadata>(),
            actionArguments: actionArguments ?? new Dictionary<string, object?>(),
            controller: new object()
        );

        return context;
    }

    public class TestClass
    {
    }
}
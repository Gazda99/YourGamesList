using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Model;
using YourGamesList.Api.ModelBinders;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.ModelBinders;

public class JwtUserInformationModelBinderTests
{
    private IFixture _fixture;
    private ILogger<JwtUserInformationModelBinder> _logger;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<JwtUserInformationModelBinder>>();
    }

    [Test]
    public async Task BindModelAsync_OnCorrectUserInformation_SuccessfullyBindsModel()
    {
        //ARRANGE
        var context = Substitute.For<ModelBindingContext>();
        var httpContext = new DefaultHttpContext();
        context.HttpContext.Returns(httpContext);
        var userInformation = _fixture.Create<JwtUserInformation>();
        context.HttpContext.Items[nameof(JwtUserInformation)] = userInformation;
        context.ModelType.Returns(typeof(JwtUserInformation));
        var binder = new JwtUserInformationModelBinder(_logger);

        //ACT
        await binder.BindModelAsync(context);

        //ASSERT
        Assert.That(context.Result.IsModelSet, Is.True);
        _logger.ReceivedLog(LogLevel.Information, [$"Successfully bound '{nameof(JwtUserInformation)}'"]);
    }

    [Test]
    public async Task BindModelAsync_OnMissingUserInformation_ThrowsModelBindingException()
    {
        //ARRANGE
        const string expectedErrorMessage = "JwtUserInformation is missing. Authentication Middleware failure or token parsing error.";
        var context = Substitute.For<ModelBindingContext>();
        var httpContext = new DefaultHttpContext();
        context.HttpContext.Returns(httpContext);
        context.HttpContext.Items = new Dictionary<object, object?>(); // Empty dict
        context.ModelType.Returns(typeof(JwtUserInformation));
        var binder = new JwtUserInformationModelBinder(_logger);

        //ACT
        var ex = Assert.ThrowsAsync<ModelBindingException>(async () => await binder.BindModelAsync(context));

        //ASSERT
        Assert.That(ex.ErrorDescription, Is.EquivalentTo(expectedErrorMessage));
        _logger.ReceivedLog(LogLevel.Error, expectedErrorMessage);
    }

    [Test]
    public void BindModelAsync_OnContextNull_ThrowsModelBindingException()
    {
        //ARRANGE
        ModelBindingContext? context = null;
        var binder = new JwtUserInformationModelBinder(_logger);

        //ACT
        var ex = Assert.ThrowsAsync<ModelBindingException>(async () => await binder.BindModelAsync(context));

        //ASSERT
        Assert.That(ex.ErrorDescription, Is.EquivalentTo("bindingContext is null"));
    }

    [Test]
    public async Task BindModelAsync_OnWrongModelType_ReturnsCompletedTask()
    {
        //ARRANGE
        var context = Substitute.For<ModelBindingContext>();
        context.ModelType.Returns(typeof(object));
        var binder = new JwtUserInformationModelBinder(_logger);

        //ACT
        await binder.BindModelAsync(context);

        //ASSERT
        _logger.ReceivedLog(LogLevel.Warning,
            $"Attempted to bind model of type '{typeof(object).Name}' with '{nameof(JwtUserInformationModelBinder)}'. This binder is only for '{nameof(JwtUserInformation)}'.");
    }
}
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.Net.Http.Headers;
using NSubstitute;
using YourGamesList.Api.Model;
using YourGamesList.Api.ModelBinders;
using YourGamesList.Api.Services.Auth;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.ModelBinders;

public class JwtUserInformationModelBinderTests
{
    private IFixture _fixture;
    private ILogger<JwtUserInformationModelBinder> _logger;
    private ITokenParser _tokenParser;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<JwtUserInformationModelBinder>>();
        _tokenParser = Substitute.For<ITokenParser>();
    }

    [Test]
    public async Task BindModelAsync_OnCorrectToken_SuccessfullyBindsModel()
    {
        //ARRANGE
        var rawToken = _fixture.Create<string>();
        //simple token with "sub" claim set to "sub" and "userId" claim set to "userId"
        var token = new JsonWebToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJzdWIiLCJ1c2VySWQiOiJhMWIyYzNkNC1lNWY2LTc4OTAtYWJjZC1lZjAxMjM0NTY3ODkifQ.9I_rQ_xO2mNfJzV74w4Y6b5rS3t4gZ0yM8d8S4_5x_0");
        _tokenParser.CanReadToken(rawToken).Returns(true);
        _tokenParser.ReadJsonWebToken(rawToken).Returns(token);
        var context = Substitute.For<ModelBindingContext>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append(HeaderNames.Authorization, $"Bearer {rawToken}");
        context.HttpContext.Returns(httpContext);
        context.ModelType.Returns(typeof(JwtUserInformation));
        var binder = new JwtUserInformationModelBinder(_logger, _tokenParser);

        //ACT
        await binder.BindModelAsync(context);

        //ASSERT
        Assert.That(context.Result.IsModelSet, Is.True);
        _logger.ReceivedLog(LogLevel.Information, [$"Successfully bound '{nameof(JwtUserInformation)}'"]);
    }

    [Test]
    public void BindModelAsync_OnContextNull_ThrowsModelBindingException()
    {
        //ARRANGE
        ModelBindingContext? context = null;
        var binder = new JwtUserInformationModelBinder(_logger, _tokenParser);

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
        var binder = new JwtUserInformationModelBinder(_logger, _tokenParser);

        //ACT
        await binder.BindModelAsync(context);

        //ASSERT
        _logger.ReceivedLog(LogLevel.Warning,
            $"Attempted to bind model of type '{typeof(object).Name}' with '{nameof(JwtUserInformationModelBinder)}'. This binder is only for '{nameof(JwtUserInformation)}'.");
    }

    [Test]
    [TestCaseSource(nameof(WrongAuthHeaderCases))]
    public async Task BindModelAsync_OnWrongAuthHeader_ThrowsModelBindingException(string headerName, string headerValue)
    {
        //ARRANGE
        const string errorMessage = "Authorization header missing or not in 'Bearer' format.";
        var context = Substitute.For<ModelBindingContext>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append(headerName, headerValue);
        context.HttpContext.Returns(httpContext);
        context.ModelType.Returns(typeof(JwtUserInformation));
        var binder = new JwtUserInformationModelBinder(_logger, _tokenParser);

        //ACT
        var ex = Assert.ThrowsAsync<ModelBindingException>(async () => await binder.BindModelAsync(context));

        //ASSERT
        Assert.That(context.Result.IsModelSet, Is.False);
        Assert.That(ex.ErrorDescription, Is.EquivalentTo(errorMessage));
        Assert.That(ex.ModelType, Is.EqualTo(typeof(JwtUserInformation)));
        _logger.ReceivedLog(LogLevel.Information, errorMessage);
    }

    private static readonly TestCaseData[] WrongAuthHeaderCases =
    [
        new TestCaseData(HeaderNames.Authorization, "wrong-value"),
        new TestCaseData("NoAuthHeaderExample", "whatever")
    ];

    [Test]
    public async Task BindModelAsync_OnUnparsableToken_ThrowsModelBindingException()
    {
        //ARRANGE
        const string errorMessage = "Cannot read JWT from authorization header.";
        var unparsableToken = _fixture.Create<string>();
        _tokenParser.CanReadToken(unparsableToken).Returns(false);
        var context = Substitute.For<ModelBindingContext>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append(HeaderNames.Authorization, $"Bearer {unparsableToken}");
        context.HttpContext.Returns(httpContext);
        context.ModelType.Returns(typeof(JwtUserInformation));
        var binder = new JwtUserInformationModelBinder(_logger, _tokenParser);

        //ACT
        var ex = Assert.ThrowsAsync<ModelBindingException>(async () => await binder.BindModelAsync(context));

        //ASSERT
        Assert.That(context.Result.IsModelSet, Is.False);
        Assert.That(ex.ErrorDescription, Is.EquivalentTo(errorMessage));
        Assert.That(ex.ModelType, Is.EqualTo(typeof(JwtUserInformation)));
        _logger.ReceivedLog(LogLevel.Information, errorMessage);
    }

    [Test]
    [TestCaseSource(nameof(TokensWithMissingClaimsCases))]
    public async Task BindModelAsync_OnTokenWithMissingClaims_SuccessfullyBindsModel(string rawToken, string missingClaimType)
    {
        //ARRANGE
        var errorMessage = $"Cannot read JWT '{missingClaimType}' claim.";
        var token = new JsonWebToken(rawToken);
        _tokenParser.CanReadToken(rawToken).Returns(true);
        _tokenParser.ReadJsonWebToken(rawToken).Returns(token);
        var context = Substitute.For<ModelBindingContext>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append(HeaderNames.Authorization, $"Bearer {rawToken}");
        context.HttpContext.Returns(httpContext);
        context.ModelType.Returns(typeof(JwtUserInformation));
        var binder = new JwtUserInformationModelBinder(_logger, _tokenParser);

        //ACT
        var ex = Assert.ThrowsAsync<ModelBindingException>(async () => await binder.BindModelAsync(context));

        //ASSERT
        Assert.That(context.Result.IsModelSet, Is.False);
        Assert.That(ex.ErrorDescription, Is.EquivalentTo(errorMessage));
        Assert.That(ex.ModelType, Is.EqualTo(typeof(JwtUserInformation)));
        _logger.ReceivedLog(LogLevel.Information, errorMessage);
    }

    private static readonly TestCaseData[] TokensWithMissingClaimsCases =
    [
        new TestCaseData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiJhMWIyYzNkNC1lNWY2LTc4OTAtYWJjZC1lZjAxMjM0NTY3ODkifQ.YmX5c-kQ-wz6Z-4x-B2jJ7b9pT8sZ0yM8d8S4_5x_0", "sub"),
        new TestCaseData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJzdWIifQ.7d01h_3u3f-2fA5p0qW_t3u9t6b5rS3t4gZ0yM8d8S4_5x_0", "userId")
    ];
}
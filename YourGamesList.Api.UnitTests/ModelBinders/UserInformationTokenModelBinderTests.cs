using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using NSubstitute;
using YourGamesList.Api.Model;
using YourGamesList.Api.ModelBinders;
using YourGamesList.Api.Services.Auth;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.ModelBinders;

[TestFixture]
public class UserInformationTokenModelBinderTests
{
    private ILogger<UserInformationTokenModelBinder> _logger;
    private UserInformationTokenModelBinder _binder;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<UserInformationTokenModelBinder>>();
        _binder = new UserInformationTokenModelBinder(_logger);
    }

    [Test]
    public async Task BindModelAsync_OnValidUserClaims_SuccessfullyBindsModel()
    {
        //ARRANGE
        var expectedUserId = Guid.NewGuid();
        const string expectedNormalizedUsername = "gazda";
        const string expectedRawToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.sample.token";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, expectedNormalizedUsername),
            new Claim(JwtCustomClaimNames.UserId, expectedUserId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };
        httpContext.Request.Headers.Authorization = $"Bearer {expectedRawToken}";

        var context = Substitute.For<ModelBindingContext>();
        context.HttpContext.Returns(httpContext);
        context.ModelType.Returns(typeof(UserInformationToken));

        //ACT
        await _binder.BindModelAsync(context);

        //ASSERT
        Assert.That(context.Result.IsModelSet, Is.True);

        var boundToken = context.Result.Model as UserInformationToken;
        Assert.That(boundToken, Is.Not.Null);
        Assert.That(boundToken!.RawToken, Is.EqualTo(expectedRawToken));
        Assert.That(boundToken.NormalizedUsername, Is.EqualTo(expectedNormalizedUsername));
        Assert.That(boundToken.UserId, Is.EqualTo(expectedUserId));
    }

    [Test]
    public async Task BindModelAsync_OnMissingOrInvalidClaims_SetsFailedResult()
    {
        //ARRANGE
        var httpContext = new DefaultHttpContext(); // Default HttpContext has unauthenticated user without claims

        var context = Substitute.For<ModelBindingContext>();
        context.HttpContext.Returns(httpContext);
        context.ModelType.Returns(typeof(UserInformationToken));

        //ACT - ASSERT
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _binder.BindModelAsync(context!));
        _logger.ReceivedLog(LogLevel.Warning, ["User token cannot be parsed. Error"]);
    }

    [Test]
    public async Task BindModelAsync_OnContextNull_ThrowsArgumentNullException()
    {
        //ARRANGE
        ModelBindingContext? context = null;

        //ACT & ASSERT
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await _binder.BindModelAsync(context!));
    }

    [Test]
    public async Task BindModelAsync_OnWrongModelType_ReturnsCompletedTaskWithoutSettingResult()
    {
        //ARRANGE
        var context = Substitute.For<ModelBindingContext>();
        context.ModelType.Returns(typeof(object));

        //ACT
        await _binder.BindModelAsync(context);

        //ASSERT
        Assert.That(context.Result, Is.EqualTo(default(ModelBindingResult)));
    }
}
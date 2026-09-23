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
        const string expectedUsername = "Gazda";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, expectedUsername),
            new Claim(JwtCustomClaimNames.UserId, expectedUserId.ToString())
        };

        // Note: Providing authenticationType ("TestAuth") sets IsAuthenticated to true
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        var context = Substitute.For<ModelBindingContext>();
        context.HttpContext.Returns(httpContext);
        context.ModelType.Returns(typeof(UserInformationToken));

        //ACT
        await _binder.BindModelAsync(context);

        //ASSERT
        Assert.That(context.Result.IsModelSet, Is.True);

        var boundToken = context.Result.Model as UserInformationToken;
        Assert.That(boundToken, Is.Not.Null);
        Assert.That(boundToken!.Username, Is.EqualTo(expectedUsername));
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
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _binder.BindModelAsync(context!));
        _logger.ReceivedLog(LogLevel.Warning, ["User token cannot be parsed. Error"]);
    }

    [Test]
    public void BindModelAsync_OnContextNull_ThrowsArgumentNullException()
    {
        //ARRANGE
        ModelBindingContext? context = null;

        //ACT & ASSERT
        Assert.ThrowsAsync<ArgumentNullException>(async () => await _binder.BindModelAsync(context!));
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
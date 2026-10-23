using System;
using System.Collections.Generic;
using System.Security.Claims;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using YourGamesList.Api.Model;
using YourGamesList.Api.Services.Auth;

namespace YourGamesList.Api.UnitTests.Model;

[TestFixture]
public class UserInformationTokenTests
{
    private IFixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void FromHttpContext_WhenUserNotAuthenticated_ReturnsFailure()
    {
        //ARRANGE
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity())
        };

        //ACT
        var result = UserInformationToken.FromHttpContext(httpContext);

        //ASSERT
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("  ")]
    [TestCase("Basic xyz123")]
    public void FromHttpContext_WhenMissingOrInvalidAuthorizationHeader_ReturnsFailure(string? authHeader)
    {
        //ARRANGE
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "Gazda"),
            new Claim(JwtCustomClaimNames.UserId, Guid.NewGuid().ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        if (authHeader != null)
        {
            httpContext.Request.Headers.Authorization = authHeader;
        }

        //ACT
        var result = UserInformationToken.FromHttpContext(httpContext);

        //ASSERT
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("  ")]
    public void FromHttpContext_WhenInvalidUsername_ReturnsFailure(string? username)
    {
        //ARRANGE
        var userId = Guid.NewGuid().ToString();
        var claims = new List<Claim>();

        if (username != null)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, username));
        }

        claims.Add(new Claim(JwtCustomClaimNames.UserId, userId));

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
        httpContext.Request.Headers.Authorization = "Bearer sample.raw.jwt";

        //ACT
        var result = UserInformationToken.FromHttpContext(httpContext);

        //ASSERT
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("  ")]
    [TestCase("not-a-guid")]
    [TestCase("00000000-0000-0000-0000-000000000000")] // Guid.Empty
    public void FromHttpContext_WhenInvalidUserId_ReturnsFailure(string? userId)
    {
        //ARRANGE
        var username = _fixture.Create<string>();
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, username)
        };

        if (userId != null)
        {
            claims.Add(new Claim(JwtCustomClaimNames.UserId, userId));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
        httpContext.Request.Headers.Authorization = "Bearer sample.raw.jwt";

        //ACT
        var result = UserInformationToken.FromHttpContext(httpContext);

        //ASSERT
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public void FromHttpContext_WhenValidRequest_ReturnsSuccessResult()
    {
        //ARRANGE
        const string expectedNormalizedUsername = "testuser";
        var expectedUserId = Guid.NewGuid();
        const string expectedRawToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.sample.token";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, expectedNormalizedUsername),
            new Claim(JwtCustomClaimNames.UserId, expectedUserId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
        httpContext.Request.Headers.Authorization = $"Bearer {expectedRawToken}";

        //ACT
        var result = UserInformationToken.FromHttpContext(httpContext);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.RawToken, Is.EqualTo(expectedRawToken));
        Assert.That(result.Value.NormalizedUsername, Is.EqualTo(expectedNormalizedUsername));
        Assert.That(result.Value.UserId, Is.EqualTo(expectedUserId));
    }
}
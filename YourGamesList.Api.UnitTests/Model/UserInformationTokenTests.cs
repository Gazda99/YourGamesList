using System;
using System.Collections.Generic;
using System.Security.Claims;
using AutoFixture;
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
    public void FromClaimsPrincipal_WhenUserNotAuthenticated_ReturnsFailure()
    {
        // ARRANGE
        var unauthenticatedPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

        // ACT
        var result = UserInformationToken.FromClaimsPrincipal(unauthenticatedPrincipal);

        // ASSERT
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("  ")]
    public void FromClaimsPrincipal_WhenInvalidUsername_ReturnsFailure(string? username)
    {
        // ARRANGE
        var userId = Guid.NewGuid().ToString();
        var claims = new List<Claim>();

        if (username != null)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, username));
        }

        claims.Add(new Claim(JwtCustomClaimNames.UserId, userId));

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        // ACT
        var result = UserInformationToken.FromClaimsPrincipal(claimsPrincipal);

        // ASSERT
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("  ")]
    [TestCase("not-a-guid")]
    [TestCase("00000000-0000-0000-0000-000000000000")] // Guid.Empty
    public void FromClaimsPrincipal_WhenInvalidUserId_ReturnsFailure(string? userId)
    {
        // ARRANGE
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
        var claimsPrincipal = new ClaimsPrincipal(identity);

        // ACT
        var result = UserInformationToken.FromClaimsPrincipal(claimsPrincipal);

        // ASSERT
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public void FromClaimsPrincipal_WhenValidClaimsWithSub_ReturnsSuccessResult()
    {
        // ARRANGE
        var expectedUsername = _fixture.Create<string>();
        var expectedUserId = Guid.NewGuid();

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, expectedUsername),
            new Claim(JwtCustomClaimNames.UserId, expectedUserId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        // ACT
        var result = UserInformationToken.FromClaimsPrincipal(claimsPrincipal);

        // ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Username, Is.EqualTo(expectedUsername));
        Assert.That(result.Value.UserId, Is.EqualTo(expectedUserId));
    }
}
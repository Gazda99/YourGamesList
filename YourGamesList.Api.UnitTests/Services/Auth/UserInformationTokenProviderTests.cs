using System;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Api.Services.Auth.Options;

namespace YourGamesList.Api.UnitTests.Services.Auth;

[TestFixture]
public class UserInformationTokenProviderTests
{
    private IOptions<TokenAuthOptions> _options;
    private FakeTimeProvider _timeProvider;

    [SetUp]
    public void Setup()
    {
        _options = Substitute.For<IOptions<TokenAuthOptions>>();
        _timeProvider = new FakeTimeProvider();
    }

    [Test]
    public void CreateToken_WhenCalled_CreatesTokenWithExpectedClaimsAndLifetime()
    {
        //ARRANGE
        var options = new TokenAuthOptions
        {
            Audience = "yourgameslist",
            Issuer = "yourgameslist",
            JwtSecret = "test-secret-that-is-long-enough-for-hmac-sha256-signing",
            ExpirationInMinutes = 30
        };
        _options.Value.Returns(options);
        var now = DateTimeOffset.UtcNow;
        var validFrom = now.ToUnixTimeSeconds();
        var validTo = validFrom + options.ExpirationInMinutes * 60;
        _timeProvider.SetUtcNow(now);
        const string username = "TestUser";
        var userId = Guid.NewGuid();
        var handler = new JsonWebTokenHandler();
        var tokenProvider = new UserInformationTokenProvider(_options, _timeProvider);

        //ACT
        var rawToken = tokenProvider.CreateToken(username, userId);
        var token = handler.ReadJsonWebToken(rawToken);

        //ASSERT
        Assert.That(token.Claims.Single(x => x.Type == "sub").Value, Is.EqualTo(username.ToLower()));
        Assert.That(token.Claims.Single(x => x.Type == JwtCustomClaimNames.UserId).Value, Is.EqualTo(userId.ToString()));
        Assert.That(token.Issuer, Is.EqualTo(options.Issuer));
        Assert.That(token.Audiences, Does.Contain(options.Audience));
        Assert.That(token.Alg, Is.EqualTo(SecurityAlgorithms.HmacSha256));
        Assert.That(((DateTimeOffset) token.ValidFrom).ToUnixTimeSeconds(), Is.EqualTo(validFrom));
        Assert.That(((DateTimeOffset) token.ValidTo).ToUnixTimeSeconds(), Is.EqualTo(validTo));
    }
}
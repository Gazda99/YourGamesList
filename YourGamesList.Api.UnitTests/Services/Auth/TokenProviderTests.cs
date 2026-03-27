using System;
using AutoFixture;
using Microsoft.Extensions.Options;
using NSubstitute;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Api.Services.Auth.Options;

namespace YourGamesList.Api.UnitTests.Services.Auth;

public class TokenProviderTests
{
    private IFixture _fixture;
    private IOptions<TokenAuthOptions> _options;
    private TimeProvider _timeProvider;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _options = Substitute.For<IOptions<TokenAuthOptions>>();
        _timeProvider = Substitute.For<TimeProvider>();
    }

    [Test]
    public void TestTokenProvider()
    {
        //ARRANGE
        var secret = _fixture.Create<string>();
        var options = _fixture.Build<TokenAuthOptions>().With(x => x.JwtSecret, secret).WithAutoProperties().Create();
        _options.Value.Returns(options);
        var userName = _fixture.Create<string>();
        var userId = Guid.NewGuid();
        var now = DateTime.Now;
        _timeProvider.GetUtcNow().Returns(now);
        var tokenProvider = new TokenProvider(_options, _timeProvider);

        //ACT
        var token = tokenProvider.CreateToken(userName, userId);

        //ASSERT
        Assert.That(token.Split('.').Length, Is.EqualTo(3));
    }
}
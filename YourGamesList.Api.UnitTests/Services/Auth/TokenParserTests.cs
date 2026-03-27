using System;
using System.Linq;
using AutoFixture;
using YourGamesList.Api.Services.Auth;

namespace YourGamesList.Api.UnitTests.Services.Auth;

public class TokenParserTests
{
    //simple token with "sub" claim set to "sub" and "userId" claim set to "userId"
    private const string SimpleRawToken = "eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJzdWIiOiJzdWIiLCJ1c2VySWQiOiJ1c2VySWQifQ.";
    private IFixture _fixture;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void CanReadToken_CorrectToken_ReturnsTrue()
    {
        //ARRANGE
        var tokenParser = new TokenParser();

        //ACT
        var res = tokenParser.CanReadToken(SimpleRawToken);

        //ASSERT
        Assert.That(res, Is.True);
    }

    [Test]
    public void CanReadToken_InvalidToken_ReturnsTrue()
    {
        //ARRANGE
        var tokenParser = new TokenParser();

        //ACT
        var res = tokenParser.CanReadToken(_fixture.Create<string>());

        //ASSERT
        Assert.That(res, Is.False);
    }

    [Test]
    public void ReadJsonWebToken_CorrectToken_ReturnsJsonWebToken()
    {
        //ARRANGE
        var tokenParser = new TokenParser();

        //ACT
        var res = tokenParser.ReadJsonWebToken(SimpleRawToken);

        //ASSERT
        Assert.That(res.Claims, Has.Count.EqualTo(2));
        Assert.That(res.Claims.Count(x => x.Type == "sub" && x.Value == "sub"), Is.EqualTo(1));
        Assert.That(res.Claims.Count(x => x.Type == "userId" && x.Value == "userId"), Is.EqualTo(1));
    }

    [Test]
    public void ReadJsonWebToken_InvalidToken_ThrowsException()
    {
        //ARRANGE
        var tokenParser = new TokenParser();

        //ACT - ASSERT
        Assert.Catch<Exception>(() => tokenParser.ReadJsonWebToken(_fixture.Create<string>()));
    }
}
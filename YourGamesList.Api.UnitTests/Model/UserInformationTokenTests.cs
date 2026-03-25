using System;
using System.Security.Claims;
using AutoFixture;
using YourGamesList.Api.Model;
using Microsoft.IdentityModel.JsonWebTokens;
using YourGamesList.Api.Services.Auth;
using YourGamesList.TestsUtils;
using YourGamesList.TestsUtils.Assertions;

namespace YourGamesList.Api.UnitTests.Model;

public class UserInformationTokenTests : BaseTest
{
    [Test]
    [TestCase("")]
    [TestCase("  ")]
    public void FromRawToken_WhenInvalidUsername_Throws(string username)
    {
        //ARRANGE
        var userId = Guid.NewGuid().ToString();
        var (rawToken, jwt) = JwtHelper.CreateToken([
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtCustomClaimNames.UserId, userId)
            ]
        );

        //ACT
        var ex = Assert.Throws<BaseTokenCreationException>(() => UserInformationToken.FromRawJwt(rawToken));
        Assert.That(ex.Message, CollectionContains.ContainsAll(JwtRegisteredClaimNames.Sub, username));
    }

    [Test]
    [TestCase("")]
    [TestCase("  ")]
    public void FromJwt_WhenInvalidUsername_Throws(string username)
    {
        //ARRANGE
        var userId = Guid.NewGuid().ToString();
        var (rawToken, jwt) = JwtHelper.CreateToken([
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtCustomClaimNames.UserId, userId)
            ]
        );

        //ACT
        var ex = Assert.Throws<BaseTokenCreationException>(() => UserInformationToken.FromJwt(jwt));
        Assert.That(ex.Message, CollectionContains.ContainsAll(JwtRegisteredClaimNames.Sub, username));
    }

    [Test]
    [TestCase("")]
    [TestCase("  ")]
    [TestCase("not guid")]
    public void FromRawJwt_WhenInvalidUserId_Throws(string userId)
    {
        //ARRANGE
        var username = _fixture.Create<string>();
        var (rawToken, jwt) = JwtHelper.CreateToken([
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtCustomClaimNames.UserId, userId)
            ]
        );

        //ACT
        var ex = Assert.Throws<BaseTokenCreationException>(() => UserInformationToken.FromRawJwt(rawToken));
        Assert.That(ex.Message, CollectionContains.ContainsAll(JwtCustomClaimNames.UserId, userId));
    }

    [Test]
    [TestCase("")]
    [TestCase("  ")]
    [TestCase("not guid")]
    public void FromJwt_WhenInvalidUserId_Throws(string userId)
    {
        //ARRANGE
        var username = _fixture.Create<string>();
        var (rawToken, jwt) = JwtHelper.CreateToken([
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtCustomClaimNames.UserId, userId)
            ]
        );

        //ACT
        var ex = Assert.Throws<BaseTokenCreationException>(() => UserInformationToken.FromJwt(jwt));
        Assert.That(ex.Message, CollectionContains.ContainsAll(JwtCustomClaimNames.UserId, userId));
    }
}
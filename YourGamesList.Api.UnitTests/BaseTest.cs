using System;
using System.Security.Claims;
using AutoFixture;
using Microsoft.IdentityModel.JsonWebTokens;
using YourGamesList.Api.Model;
using YourGamesList.Api.Services.Auth;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests;

public abstract class BaseTest
{
    protected IFixture _fixture;

    [SetUp]
    public void BaseSetup()
    {
        _fixture = new Fixture();
        _fixture.Register(UserInformationTokenCreator);
    }

    private UserInformationToken UserInformationTokenCreator()
    {
        var username = _fixture.Create<string>();
        var userId = Guid.NewGuid().ToString();
        var jwt = JwtHelper.CreateToken([
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtCustomClaimNames.UserId, userId)
            ]
        );

        return new UserInformationToken(jwt.token);
    }
}
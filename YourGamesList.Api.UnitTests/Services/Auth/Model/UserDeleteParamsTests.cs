using System;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Api.Services.Auth.Model;
using YourGamesList.Contracts.Requests.Users;

namespace YourGamesList.Api.UnitTests.Services.Auth.Model;

public class UserDeleteParamsTests
{
    [Test]
    public void ToParams_MapsAllFields()
    {
        var userInformation = new UserInformationToken
        {
            RawToken = "token",
            Username = "testuser",
            UserId = Guid.NewGuid()
        };
        var request = new UserDeleteRequest
        {
            UserInformation = userInformation,
            Body = new AuthUserDeleteRequestBody(" testuser ", "password")
        };

        var result = request.ToParams();

        Assert.That(result.UserInformation, Is.SameAs(userInformation));
        Assert.That(result.Username, Is.EqualTo("testuser"));
        Assert.That(result.Password, Is.EqualTo("password"));
    }
}

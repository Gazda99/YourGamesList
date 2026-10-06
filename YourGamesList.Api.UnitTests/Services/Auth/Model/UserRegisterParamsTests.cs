using System;
using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Api.Services.Auth.Model;
using YourGamesList.Contracts.Requests.Users;

namespace YourGamesList.Api.UnitTests.Services.Auth.Model;

public class UserRegisterParamsTests
{
    [Test]
    public void ToParams_MapsAllFields()
    {
        var dateOfBirth = new DateOnly(1995, 3, 14);
        var request = new UserRegisterRequest
        {
            Body = new AuthUserRegisterRequestBody("testuser", "test@example.com", "password", dateOfBirth)
        };

        var result = request.ToParams();

        Assert.That(result.Username, Is.EqualTo("testuser"));
        Assert.That(result.Email, Is.EqualTo("test@example.com"));
        Assert.That(result.Password, Is.EqualTo("password"));
        Assert.That(result.DateOfBirth, Is.EqualTo(dateOfBirth));
    }
}

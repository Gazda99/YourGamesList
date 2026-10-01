using System;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Api.Services.YourGamesList.Model;

namespace YourGamesList.Api.UnitTests.Services.YourGamesList.Model;

public class UserGetSelfParamsTests
{
    [Test]
    public void ToParams_MapsUserInformation()
    {
        var userInformation = new UserInformationToken
        {
            RawToken = "token",
            Username = "testuser",
            UserId = Guid.NewGuid()
        };
        var request = new UserGetSelfRequest { UserInformation = userInformation };

        var result = request.ToParams();

        Assert.That(result.UserInformation, Is.SameAs(userInformation));
    }
}

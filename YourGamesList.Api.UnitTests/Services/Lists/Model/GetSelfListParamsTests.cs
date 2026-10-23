using System;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Services.Lists.Model;

namespace YourGamesList.Api.UnitTests.Services.Lists.Model;

public class GetSelfListParamsTests
{
    [Test]
    public void ToParams_MapsUserInformation()
    {
        var userInformation = new UserInformationToken
        {
            RawToken = "token",
            NormalizedUsername = "testuser",
            UserId = Guid.NewGuid()
        };
        var request = new GetSelfListRequest { UserInformation = userInformation };

        var result = request.ToParams();

        Assert.That(result.UserInformation, Is.SameAs(userInformation));
    }
}

using System;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Services.Lists.Model;
using YourGamesList.Contracts.Requests.Lists;

namespace YourGamesList.Api.UnitTests.Services.Lists.Model;

public class AddListParamsTests
{
    [Test]
    public void ToParams_MapsName()
    {
        var request = new AddListRequest
        {
            UserInformation = CreateUserInformation(),
            Body = new CreateListRequestBody(" My games")
        };

        var result = request.ToParams();

        Assert.That(result.Name, Is.EqualTo("My games"));
    }

    private static UserInformationToken CreateUserInformation()
    {
        return new UserInformationToken
        {
            RawToken = "token",
            NormalizedUsername = "testuser",
            UserId = Guid.NewGuid()
        };
    }
}

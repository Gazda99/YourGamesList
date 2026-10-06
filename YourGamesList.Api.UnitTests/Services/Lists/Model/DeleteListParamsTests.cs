using System;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Services.Lists.Model;
using YourGamesList.Contracts.Requests.Lists;

namespace YourGamesList.Api.UnitTests.Services.Lists.Model;

public class DeleteListParamsTests
{
    [Test]
    public void ToParams_MapsId()
    {
        var listId = Guid.NewGuid();
        var request = new DeleteListRequest
        {
            UserInformation = CreateUserInformation(),
            Body = new DeleteListRequestBody(listId)
        };

        var result = request.ToParams();

        Assert.That(result.Id, Is.EqualTo(listId));
    }

    private static UserInformationToken CreateUserInformation()
    {
        return new UserInformationToken
        {
            RawToken = "token",
            Username = "testuser",
            UserId = Guid.NewGuid()
        };
    }
}

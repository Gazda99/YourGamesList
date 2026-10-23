using System;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Services.Lists.Model;

namespace YourGamesList.Api.UnitTests.Services.Lists.Model;

public class RemoveGameListEntryParamsTests
{
    [Test]
    public void ToParams_MapsRequestValues()
    {
        //ARRANGE
        var userInformation = CreateUserInformation();
        var listId = Guid.NewGuid();
        var entryId = Guid.NewGuid();
        var request = new RemoveGameListEntryRequest
        {
            UserInformation = userInformation,
            ListId = listId,
            EntryId = entryId
        };

        //ACT
        var result = request.ToParams();

        //ASSERT
        Assert.That(result.UserInformation, Is.SameAs(userInformation));
        Assert.That(result.GamesListId, Is.EqualTo(listId));
        Assert.That(result.Id, Is.EqualTo(entryId));
    }

    private static UserInformationToken CreateUserInformation() => new()
    {
        RawToken = "token",
        Username = "testuser",
        UserId = Guid.NewGuid()
    };
}

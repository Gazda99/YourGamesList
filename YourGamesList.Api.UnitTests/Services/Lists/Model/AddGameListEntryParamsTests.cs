using System;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Services.Lists.Model;
using YourGamesList.Contracts.Dto;
using YourGamesList.Contracts.Requests.Lists;

namespace YourGamesList.Api.UnitTests.Services.Lists.Model;

public class AddGameListEntryParamsTests
{
    [Test]
    public void ToParams_MapsRequestValues()
    {
        //ARRANGE
        var userInformation = CreateUserInformation();
        var listId = Guid.NewGuid();
        var request = new AddGameListEntryRequest
        {
            UserInformation = userInformation,
            ListId = listId,
            Body = new AddGameListEntryRequestBody
            {
                CompletionStatus = CompletionStatusDto.Played,
                IgdbId = 123,
                Description = "A favorite",
                Rating = 9
            }
        };

        //ACT
        var result = request.ToParams();

        //ASSERT
        Assert.That(result.UserInformation, Is.SameAs(userInformation));
        Assert.That(result.GamesListId, Is.EqualTo(listId));
        Assert.That(result.CompletionStatus, Is.EqualTo(CompletionStatusDto.Played));
        Assert.That(result.IgdbId, Is.EqualTo(123));
        Assert.That(result.Description, Is.EqualTo("A favorite"));
        Assert.That(result.Rating, Is.EqualTo(9));
    }

    private static UserInformationToken CreateUserInformation() => new()
    {
        RawToken = "token",
        Username = "testuser",
        UserId = Guid.NewGuid()
    };
}

using System;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Services.Lists.Model;
using YourGamesList.Contracts;
using YourGamesList.Contracts.Dto;
using YourGamesList.Contracts.Requests.Lists;

namespace YourGamesList.Api.UnitTests.Services.Lists.Model;

public class UpdateGameListEntryParamsTests
{
    [Test]
    public void ToParams_MapsRequestValues()
    {
        //ARRANGE
        var userInformation = CreateUserInformation();
        var listId = Guid.NewGuid();
        var entryId = Guid.NewGuid();
        var completionStatus = Optional<CompletionStatusDto?>.Some(CompletionStatusDto.Played);
        var description = Optional<string?>.Some("A favorite");
        var rating = Optional<byte?>.Some(9);
        var request = new UpdateGameListEntryRequest
        {
            UserInformation = userInformation,
            ListId = listId,
            EntryId = entryId,
            Body = new UpdateGameListEntryRequestBody
            {
                CompletionStatus = completionStatus,
                Description = description,
                Rating = rating
            }
        };

        //ACT
        var result = request.ToParams();

        //ASSERT
        Assert.That(result.UserInformation, Is.SameAs(userInformation));
        Assert.That(result.GamesListId, Is.EqualTo(listId));
        Assert.That(result.Id, Is.EqualTo(entryId));
        Assert.That(result.CompletionStatus, Is.EqualTo(completionStatus));
        Assert.That(result.Description, Is.EqualTo(description));
        Assert.That(result.Rating, Is.EqualTo(rating));
    }

    private static UserInformationToken CreateUserInformation() => new()
    {
        RawToken = "token",
        NormalizedUsername = "testuser",
        UserId = Guid.NewGuid()
    };
}

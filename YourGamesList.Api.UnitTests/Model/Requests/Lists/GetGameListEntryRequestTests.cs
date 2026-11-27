using System;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.UnitTests.Model.Requests.Lists;

public class GetGameListEntryRequestTests
{
    [Test]
    public void Validate_ValidRequest_IsValid()
    {
        //ARRANGE
        var request = CreateRequest(Guid.NewGuid(), Guid.NewGuid());

        //ACT
        var result = new GetGameListEntryRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_EmptyListId_IsInvalid()
    {
        //ARRANGE
        var request = CreateRequest(Guid.Empty, Guid.NewGuid());

        //ACT
        var result = new GetGameListEntryRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.ListId);
    }

    [Test]
    public void Validate_EmptyEntryId_IsInvalid()
    {
        //ARRANGE
        var request = CreateRequest(Guid.NewGuid(), Guid.Empty);

        //ACT
        var result = new GetGameListEntryRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.EntryId);
    }

    [Test]
    public void Validate_UserInformationMissing_IsInvalid()
    {
        //ARRANGE
        var request = new GetGameListEntryRequest
        {
            UserInformation = null!,
            ListId = Guid.NewGuid(),
            EntryId = Guid.NewGuid()
        };

        //ACT
        var result = new GetGameListEntryRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.UserInformation);
    }

    private static GetGameListEntryRequest CreateRequest(Guid listId, Guid entryId) => new()
    {
        UserInformation = new UserInformationToken
        {
            RawToken = "token",
            NormalizedUsername = "testuser",
            UserId = Guid.NewGuid()
        },
        ListId = listId,
        EntryId = entryId
    };
}

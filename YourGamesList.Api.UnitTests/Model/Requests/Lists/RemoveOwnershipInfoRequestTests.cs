using System;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.UnitTests.Model.Requests.Lists;

public class RemoveOwnershipInfoRequestTests
{
    [Test]
    public void Validate_ValidRequest_IsValid()
    {
        //ARRANGE
        var request = CreateRequest();

        //ACT
        var result = new RemoveOwnershipInfoRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_EmptyListId_IsInvalid()
    {
        //ARRANGE
        var request = CreateRequest(listId: Guid.Empty);

        //ACT
        var result = new RemoveOwnershipInfoRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.ListId);
    }

    [Test]
    public void Validate_EmptyEntryId_IsInvalid()
    {
        //ARRANGE
        var request = CreateRequest(entryId: Guid.Empty);

        //ACT
        var result = new RemoveOwnershipInfoRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.EntryId);
    }

    [Test]
    public void Validate_EmptyOwnershipInfoId_IsInvalid()
    {
        //ARRANGE
        var request = CreateRequest(ownershipInfoId: Guid.Empty);

        //ACT
        var result = new RemoveOwnershipInfoRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.OwnershipInfoId);
    }

    private static RemoveOwnershipInfoRequest CreateRequest(
        Guid? listId = null,
        Guid? entryId = null,
        Guid? ownershipInfoId = null) => new()
    {
        UserInformation = new UserInformationToken
        {
            RawToken = "token",
            Username = "testuser",
            UserId = Guid.NewGuid()
        },
        ListId = listId ?? Guid.NewGuid(),
        EntryId = entryId ?? Guid.NewGuid(),
        OwnershipInfoId = ownershipInfoId ?? Guid.NewGuid()
    };
}

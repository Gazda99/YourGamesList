using System;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Contracts.Requests.Lists;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.UnitTests.Model.Requests.Lists;

public class TransferGameListEntryRequestTests
{
    [Test]
    public void Validate_ValidRequest_IsValid()
    {
        //ARRANGE
        var request = CreateRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        //ACT
        var result = new TransferGameListEntryRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_EmptyEntryId_IsInvalid()
    {
        //ARRANGE
        var request = CreateRequest(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());

        //ACT
        var result = new TransferGameListEntryRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.EntryId);
    }

    [Test]
    public void Validate_EmptyTargetListId_IsInvalid()
    {
        //ARRANGE
        var request = CreateRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);

        //ACT
        var result = new TransferGameListEntryRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.TargetListId)
            .WithErrorCode(ErrorCodes.Lists.InvalidTransferTargetList);
    }

    [Test]
    public void Validate_TargetListSameAsSourceList_IsInvalid()
    {
        //ARRANGE
        var listId = Guid.NewGuid();
        var request = CreateRequest(listId, Guid.NewGuid(), listId);

        //ACT
        var result = new TransferGameListEntryRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.TargetListId)
            .WithErrorCode(ErrorCodes.Lists.InvalidTransferTargetList);
    }

    [Test]
    public void Validate_NullBody_IsInvalid()
    {
        //ARRANGE
        var request = new TransferGameListEntryRequest
        {
            UserInformation = CreateUserInformation(),
            ListId = Guid.NewGuid(),
            EntryId = Guid.NewGuid(),
            Body = null!
        };

        //ACT
        var result = new TransferGameListEntryRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body)
            .WithErrorCode(ErrorCodes.Validation.RequestBodyRequired);
    }

    private static TransferGameListEntryRequest CreateRequest(Guid listId, Guid entryId, Guid targetListId) => new()
    {
        UserInformation = CreateUserInformation(),
        ListId = listId,
        EntryId = entryId,
        Body = new TransferGameListEntryRequestBody
        {
            TargetListId = targetListId,
            Move = true
        }
    };

    private static UserInformationToken CreateUserInformation() => new()
    {
        RawToken = "token",
        NormalizedUsername = "testuser",
        UserId = Guid.NewGuid()
    };
}

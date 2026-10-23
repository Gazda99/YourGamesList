using System;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Contracts;
using YourGamesList.Contracts.Dto;
using YourGamesList.Contracts.Requests.Lists;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.UnitTests.Model.Requests.Lists;

public class UpdateOwnershipInfoRequestTests
{
    [Test]
    public void Validate_EmulatedOnWithWasEmulatedFalse_IsInvalid()
    {
        //ARRANGE
        var request = CreateRequest(new UpdateOwnershipInfoRequestBody
        {
            WasEmulated = Optional<bool?>.Some(false),
            EmulatedOn = Optional<EmulatorDto?>.Some(EmulatorDto.Xenia)
        });

        //ACT
        var result = new UpdateOwnershipInfoRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.EmulatedOn)
            .WithErrorCode(ErrorCodes.Lists.InvalidOwnershipInfoEmulation);
    }

    [Test]
    public void Validate_WasEmulatedFalseAndEmulatedOnCleared_IsValid()
    {
        //ARRANGE
        var request = CreateRequest(new UpdateOwnershipInfoRequestBody
        {
            WasEmulated = Optional<bool?>.Some(false),
            EmulatedOn = Optional<EmulatorDto?>.Some(null)
        });

        //ACT
        var result = new UpdateOwnershipInfoRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_OnlyEmulatedOnProvided_IsValid()
    {
        //ARRANGE
        var request = CreateRequest(new UpdateOwnershipInfoRequestBody
        {
            EmulatedOn = Optional<EmulatorDto?>.Some(EmulatorDto.Xenia)
        });

        //ACT
        var result = new UpdateOwnershipInfoRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    private static UpdateOwnershipInfoRequest CreateRequest(UpdateOwnershipInfoRequestBody body) => new()
    {
        UserInformation = CreateUserInformation(),
        ListId = Guid.NewGuid(),
        EntryId = Guid.NewGuid(),
        OwnershipInfoId = Guid.NewGuid(),
        Body = body
    };

    private static UserInformationToken CreateUserInformation() => new()
    {
        RawToken = "token",
        Username = "testuser",
        UserId = Guid.NewGuid()
    };
}

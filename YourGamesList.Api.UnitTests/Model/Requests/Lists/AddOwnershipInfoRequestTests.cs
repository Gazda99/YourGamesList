using System;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;

using YourGamesList.Contracts.Dto;
using YourGamesList.Contracts.Requests.Lists;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.UnitTests.Model.Requests.Lists;

public class AddOwnershipInfoRequestTests
{
    [TestCase(true, EmulatorDto.Xenia)]
    [TestCase(true, null)]
    [TestCase(false, null)]
    public void Validate_ConsistentEmulation_IsValid(bool wasEmulated, EmulatorDto? emulatedOn)
    {
        //ARRANGE
        var request = CreateRequest(wasEmulated, emulatedOn);

        //ACT
        var result = new AddOwnershipInfoRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_EmulatedOnWithoutWasEmulated_IsInvalid()
    {
        //ARRANGE
        var request = CreateRequest(false, EmulatorDto.Xenia);

        //ACT
        var result = new AddOwnershipInfoRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.EmulatedOn)
            .WithErrorCode(ErrorCodes.Lists.InvalidOwnershipInfoEmulation);
    }

    private static AddOwnershipInfoRequest CreateRequest(bool wasEmulated, EmulatorDto? emulatedOn) => new()
    {
        UserInformation = CreateUserInformation(),
        ListId = Guid.NewGuid(),
        EntryId = Guid.NewGuid(),
        Body = new AddOwnershipInfoRequestBody
        {
            Platform = PlatformDto.Unspecified,
            GameDistribution = GameDistributionDto.Unspecified,
            WasEmulated = wasEmulated,
            EmulatedOn = emulatedOn
        }
    };

    private static UserInformationToken CreateUserInformation() => new()
    {
        RawToken = "token",
        Username = "testuser",
        UserId = Guid.NewGuid()
    };
}

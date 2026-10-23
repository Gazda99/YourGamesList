using System;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.UnitTests.Model.Requests.Lists;

public class RemoveGameListEntryRequestTests
{
    [Test]
    public void Validate_EmptyEntryId_IsInvalid()
    {
        //ARRANGE
        var request = new RemoveGameListEntryRequest
        {
            UserInformation = new UserInformationToken
            {
                RawToken = "token",
                NormalizedUsername = "testuser",
                UserId = Guid.NewGuid()
            },
            ListId = Guid.NewGuid(),
            EntryId = Guid.Empty
        };

        //ACT
        var result = new RemoveGameListEntryRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.EntryId);
    }
}

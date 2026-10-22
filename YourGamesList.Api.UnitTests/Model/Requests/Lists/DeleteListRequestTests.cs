using System;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.UnitTests.Model.Requests.Lists;

public class DeleteListRequestTests
{
    [Test]
    public void Validate_ValidRequest_ReturnsValid()
    {
        //ARRANGE
        var request = new DeleteListRequest
        {
            UserInformation = new UserInformationToken
            {
                RawToken = "token",
                Username = "testuser",
                UserId = Guid.NewGuid()
            },
            ListId = Guid.NewGuid()
        };
        var validator = new DeleteListRequestValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_EmptyListId_ReturnsValidationError()
    {
        //ARRANGE
        var request = new DeleteListRequest
        {
            UserInformation = new UserInformationToken
            {
                RawToken = "token",
                Username = "testuser",
                UserId = Guid.NewGuid()
            },
            ListId = Guid.Empty
        };
        var validator = new DeleteListRequestValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.ListId);
    }

    [Test]
    public void Validate_UserInformationMissing_ReturnsValidationError()
    {
        //ARRANGE
        var request = new DeleteListRequest
        {
            UserInformation = null!,
            ListId = Guid.NewGuid()
        };
        var validator = new DeleteListRequestValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.UserInformation);
    }
}

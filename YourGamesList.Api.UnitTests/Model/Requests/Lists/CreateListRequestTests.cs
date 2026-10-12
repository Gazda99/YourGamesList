using System;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Contracts.Requests.Lists;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.UnitTests.Model.Requests.Lists;

public class CreateListRequestTests
{
    [Test]
    public void Validate_ValidRequest_ReturnsValid()
    {
        //ARRANGE
        var request = new CreateListRequest
        {
            UserInformation = new UserInformationToken
            {
                RawToken = "token",
                Username = "testuser",
                UserId = Guid.NewGuid()
            },
            Body = new CreateListRequestBody("My games")
        };
        var validator = new CreateListRequestValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_EmptyName_ReturnsValidationError()
    {
        //ARRANGE
        var request = new CreateListRequest
        {
            UserInformation = new UserInformationToken
            {
                RawToken = "token",
                Username = "testuser",
                UserId = Guid.NewGuid()
            },
            Body = new CreateListRequestBody(string.Empty)
        };
        var validator = new CreateListRequestValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.Name);
    }

    [Test]
    public void Validate_BodyMissing_ReturnsValidationError()
    {
        //ARRANGE
        var request = new CreateListRequest
        {
            UserInformation = new UserInformationToken
            {
                RawToken = "token",
                Username = "testuser",
                UserId = Guid.NewGuid()
            },
            Body = null!
        };
        var validator = new CreateListRequestValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body)
            .WithErrorCode(ErrorCodes.Validation.RequestBodyRequired);
    }

    [Test]
    public void Validate_UserInformationMissing_ReturnsValidationError()
    {
        //ARRANGE
        var request = new CreateListRequest
        {
            UserInformation = null!,
            Body = new CreateListRequestBody("My games")
        };
        var validator = new CreateListRequestValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.UserInformation);
    }
}

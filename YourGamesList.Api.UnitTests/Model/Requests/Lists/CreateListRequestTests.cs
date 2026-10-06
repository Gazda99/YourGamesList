using System;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Contracts.Requests.Lists;

namespace YourGamesList.Api.UnitTests.Model.Requests.Lists;

public class CreateListRequestTests
{
    [Test]
    public void Validate_ValidRequest_ReturnsValid()
    {
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

        var result = validator.TestValidate(request);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_EmptyName_ReturnsValidationError()
    {
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

        var result = validator.TestValidate(request);

        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.Name);
    }

    [Test]
    public void Validate_BodyMissing_ReturnsValidationError()
    {
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

        var result = validator.TestValidate(request);

        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body);
    }
}

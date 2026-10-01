using System;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Users;

namespace YourGamesList.Api.UnitTests.Model.Requests.Users;

public class UserGetSelfRequestTests
{
    [Test]
    public void Validate_UserInformationPresent_ReturnsValid()
    {
        var request = new UserGetSelfRequest
        {
            UserInformation = new UserInformationToken
            {
                RawToken = "token",
                Username = "testuser",
                UserId = Guid.NewGuid()
            }
        };
        var validator = new UserGetSelfRequestValidator();

        var result = validator.TestValidate(request);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_UserInformationMissing_ReturnsValidationError()
    {
        var request = new UserGetSelfRequest
        {
            UserInformation = null!
        };
        var validator = new UserGetSelfRequestValidator();

        var result = validator.TestValidate(request);

        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.UserInformation);
    }
}

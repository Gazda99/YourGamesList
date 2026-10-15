using System;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.UnitTests.Model.Requests.Lists;

public class GetSelfListRequestTests
{
    [Test]
    public void Validate_UserInformationPresent_ReturnsValid()
    {
        //ARRANGE
        var request = new GetSelfListRequest
        {
            UserInformation = new UserInformationToken
            {
                RawToken = "token",
                Username = "testuser",
                UserId = Guid.NewGuid()
            }
        };
        var validator = new GetSelfListRequestValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_UserInformationMissing_ReturnsValidationError()
    {
        //ARRANGE
        var request = new GetSelfListRequest
        {
            UserInformation = null!
        };
        var validator = new GetSelfListRequestValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.UserInformation);
    }
}

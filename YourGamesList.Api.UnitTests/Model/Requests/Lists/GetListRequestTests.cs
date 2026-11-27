using System;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.UnitTests.Model.Requests.Lists;

public class GetListRequestTests
{
    [Test]
    public void Validate_ValidRequest_ReturnsValid()
    {
        //ARRANGE
        var request = new GetListRequest
        {
            UserInformation = CreateUserInformation(),
            ListId = Guid.NewGuid()
        };

        //ACT
        var result = new GetListRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_EmptyListId_ReturnsValidationError()
    {
        //ARRANGE
        var request = new GetListRequest
        {
            UserInformation = CreateUserInformation(),
            ListId = Guid.Empty
        };

        //ACT
        var result = new GetListRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.ListId);
    }

    [Test]
    public void Validate_UserInformationMissing_ReturnsValidationError()
    {
        //ARRANGE
        var request = new GetListRequest
        {
            UserInformation = null!,
            ListId = Guid.NewGuid()
        };

        //ACT
        var result = new GetListRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.UserInformation);
    }

    private static UserInformationToken CreateUserInformation() => new()
    {
        RawToken = "token",
        NormalizedUsername = "testuser",
        UserId = Guid.NewGuid()
    };
}

using AutoFixture;
using FluentValidation.TestHelper;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Contracts.Requests.Users;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.UnitTests.Model.Requests.Users;

public class UserDeleteRequestTests
{
    private IFixture _fixture;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void Validate_ValidOptions_ReturnsTrue()
    {
        //ARRANGE
        var options = _fixture.Build<UserDeleteRequest>()
            .WithAutoProperties()
            .Create();

        var validator = new UserDeleteRequestValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.True);
    }

    [Test]
    public void Validate_UserInformationMissing_ReturnsValidationError()
    {
        //ARRANGE
        var request = _fixture.Build<UserDeleteRequest>()
            .With(x => x.UserInformation, (UserInformationToken)null!)
            .Create();
        var validator = new UserDeleteRequestValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.UserInformation);
    }

    [Test]
    public void Validate_BodyMissing_ReturnsValidationError()
    {
        //ARRANGE
        var request = new UserDeleteRequest { UserInformation = _fixture.Create<UserInformationToken>(), Body = null! };
        var validator = new UserDeleteRequestValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body)
            .WithErrorCode(ErrorCodes.Validation.RequestBodyRequired);
    }

    [Test]
    public void Validate_InvalidUsername_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<UserDeleteRequest>()
            .With(x => x.Body, _fixture.Build<AuthUserDeleteRequestBody>()
                .With(x => x.Username, string.Empty)
                .WithAutoProperties()
                .Create()
            )
            .WithAutoProperties()
            .Create();

        var validator = new UserDeleteRequestValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.Body.Username)
            .WithErrorCode(ErrorCodes.UserAuth.InvalidUsername);
    }

    [Test]
    public void Validate_InvalidPassword_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<UserDeleteRequest>()
            .With(x => x.Body, _fixture.Build<AuthUserDeleteRequestBody>()
                .With(x => x.Password, string.Empty)
                .WithAutoProperties()
                .Create()
            )
            .WithAutoProperties()
            .Create();

        var validator = new UserDeleteRequestValidator();

        //ACT
        var res = validator.TestValidate(options);

        //ASSERT
        Assert.That(res.IsValid, Is.False);
        Assert.That(res.Errors, Is.Not.Null);
        res.ShouldHaveValidationErrorFor(x => x.Body.Password)
            .WithErrorCode(ErrorCodes.UserAuth.InvalidPassword);
    }
}
using AutoFixture;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Contracts.Requests.Users;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.UnitTests.Model.Requests.Users;

public class UserChangePasswordRequestTests
{
    private const int MinimumPasswordLength = 8;
    private const int MaximumPasswordLength = 20;

    private IFixture _fixture;
    private ILogger<UserChangePasswordRequestValidator> _logger;
    private IOptions<RequestValidatorOptions> _options;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<UserChangePasswordRequestValidator>>();
        _options = Substitute.For<IOptions<RequestValidatorOptions>>();
        _options.Value.Returns(new RequestValidatorOptions
        {
            Password = new PasswordValidatorOptions
            {
                MinimumPasswordLength = MinimumPasswordLength,
                MaximumPasswordLength = MaximumPasswordLength
            }
        });
    }

    [Test]
    public void Validate_ValidRequest_ReturnsValid()
    {
        //ARRANGE
        var request = CreateRequest("current_password", "new_password");
        var validator = CreateValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_UserInformationMissing_ReturnsValidationError()
    {
        //ARRANGE
        var request = new UserChangePasswordRequest
        {
            UserInformation = null!,
            Body = new AuthUserChangePasswordRequestBody("current_password", "new_password")
        };
        var validator = CreateValidator();

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
        var request = new UserChangePasswordRequest { UserInformation = _fixture.Create<UserInformationToken>(), Body = null! };
        var validator = CreateValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body)
            .WithErrorCode(ErrorCodes.Validation.RequestBodyRequired);
    }

    [TestCase("")]
    [TestCase(null)]
    public void Validate_CurrentPasswordMissing_ReturnsInvalidPassword(string? currentPassword)
    {
        //ARRANGE
        var request = CreateRequest(currentPassword!, "new_password");
        var validator = CreateValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.CurrentPassword)
            .WithErrorCode(ErrorCodes.UserAuth.InvalidPassword);
    }

    [TestCase("")]
    [TestCase(null)]
    public void Validate_NewPasswordMissing_ReturnsInvalidPassword(string? newPassword)
    {
        //ARRANGE
        var request = CreateRequest("current_password", newPassword!);
        var validator = CreateValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.NewPassword)
            .WithErrorCode(ErrorCodes.UserAuth.InvalidPassword);
    }

    [Test]
    public void Validate_NewPasswordTooShort_ReturnsPasswordIsTooShort()
    {
        //ARRANGE
        var request = CreateRequest("current_password", new string('a', MinimumPasswordLength - 1));
        var validator = CreateValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.NewPassword)
            .WithErrorCode(ErrorCodes.UserAuth.PasswordIsTooShort);
    }

    [Test]
    public void Validate_NewPasswordTooLong_ReturnsPasswordIsTooLong()
    {
        //ARRANGE
        var request = CreateRequest("current_password", new string('a', MaximumPasswordLength + 1));
        var validator = CreateValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.NewPassword)
            .WithErrorCode(ErrorCodes.UserAuth.PasswordIsTooLong);
    }

    [Test]
    public void Validate_NewPasswordSameAsCurrent_ReturnsNewPasswordSameAsCurrent()
    {
        //ARRANGE
        var request = CreateRequest("same_password", "same_password");
        var validator = CreateValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.NewPassword)
            .WithErrorCode(ErrorCodes.UserAuth.NewPasswordSameAsCurrent);
    }

    [Test]
    public void Validate_NewPasswordDiffersOnlyByCase_ReturnsValid()
    {
        //ARRANGE
        var request = CreateRequest("same_password", "SAME_PASSWORD");
        var validator = CreateValidator();

        //ACT
        var result = validator.TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    private UserChangePasswordRequest CreateRequest(string currentPassword, string newPassword)
    {
        return new UserChangePasswordRequest
        {
            UserInformation = _fixture.Create<UserInformationToken>(),
            Body = new AuthUserChangePasswordRequestBody(currentPassword, newPassword)
        };
    }

    private UserChangePasswordRequestValidator CreateValidator()
    {
        return new UserChangePasswordRequestValidator(_logger, _options);
    }
}

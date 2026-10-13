using System;
using AutoFixture;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Contracts.Requests.Users;
using YourGamesList.Contracts.Responses;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Model.Requests.Users;

public class UserRegisterRequestTests
{
    private IFixture _fixture;
    private ILogger<UserRegisterRequestValidator> _logger;
    private IOptions<UserValuesValidatorOptions> _usernameValidatorOptions;
    private FakeTimeProvider _timeProvider;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<UserRegisterRequestValidator>>();
        _usernameValidatorOptions = Substitute.For<IOptions<UserValuesValidatorOptions>>();

        var options = new UserValuesValidatorOptions
        {
            Email = new EmailValidatorOptions { MaximumEmailLength = 254 },
            Username = new UsernameValidatorOptions
            {
                AllowedCharactersRegex = "^[a-zA-Z0-9]+$",
                MinimumUsernameLength = 3,
                MaximumUsernameLength = 11
            },
            Password = new PasswordValidatorOptions
            {
                MinimumPasswordLength = 8,
                MaximumPasswordLength = 20
            },
            DateOfBirth = new DateOfBirthValidatorOptions
            {
                MinimumAge = 18,
                MaximumAge = 120
            }
        };

        _usernameValidatorOptions.Value.Returns(options);
        _timeProvider = new FakeTimeProvider();
        _timeProvider.SetUtcNow(DateTimeOffset.UtcNow);
    }

    [Test]
    public void Validate_ValidRequest_ReturnsValid()
    {
        //ARRANGE
        var validator = new UserRegisterRequestValidator(_logger, _usernameValidatorOptions, _timeProvider);

        //ACT
        var result = validator.TestValidate(CreateRequest());

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_UsernameWithWhitespace_ReturnsValid()
    {
        //ARRANGE
        var validator = new UserRegisterRequestValidator(_logger, _usernameValidatorOptions, _timeProvider);
        
        //ACT
        var result = validator.TestValidate(CreateRequest(username: "  validuser  "));

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_BodyMissing_ReturnsValidationError()
    {
        //ARRANGE
        var validator = new UserRegisterRequestValidator(_logger, _usernameValidatorOptions, _timeProvider);

        //ACT
        var result = validator.TestValidate(new UserRegisterRequest { Body = null! });

        //ASSERT
        result.ShouldHaveValidationErrorFor(x => x.Body)
            .WithErrorCode(ErrorCodes.Validation.RequestBodyRequired);
    }

    [Test]
    [TestCase("", ErrorCodes.UserAuth.InvalidUsername)]
    [TestCase("ab", ErrorCodes.UserAuth.UsernameIsTooShort)]
    [TestCase("  ab  ", ErrorCodes.UserAuth.UsernameIsTooShort)]
    [TestCase("abcdefghijkl", ErrorCodes.UserAuth.UsernameIsTooLong)]
    [TestCase("bad-name", ErrorCodes.UserAuth.InvalidUsername)]
    public void Validate_InvalidUsername_ReturnsValidationError(string username, string errorCode)
    {
        //ARRANGE
        var validator = new UserRegisterRequestValidator(_logger, _usernameValidatorOptions, _timeProvider);

        //ACT
        var result = validator.TestValidate(CreateRequest(username: username));

        //ASSERT
        result.ShouldHaveValidationErrorFor(x => x.Body.Username)
            .WithErrorCode(errorCode);
    }

    [Test]
    [TestCase("")]
    [TestCase("not_an_email")]
    public void Validate_InvalidEmail_ReturnsValidationError(string email)
    {
        //ARRANGE
        var validator = new UserRegisterRequestValidator(_logger, _usernameValidatorOptions, _timeProvider);

        //ACT
        var result = validator.TestValidate(CreateRequest(email: email));

        //ASSERT
        result.ShouldHaveValidationErrorFor(x => x.Body.Email)
            .WithErrorCode(ErrorCodes.UserAuth.InvalidEmail);
    }

    [Test]
    public void Validate_EmailTooLong_ReturnsValidationError()
    {
        //ARRANGE
        var email = $"{new string('a', 250)}@b.com";

        var validator = new UserRegisterRequestValidator(_logger, _usernameValidatorOptions, _timeProvider);

        //ACT
        var result = validator.TestValidate(CreateRequest(email: email));

        //ASSERT
        result.ShouldHaveValidationErrorFor(x => x.Body.Email)
            .WithErrorCode(ErrorCodes.UserAuth.EmailIsTooLong);
    }

    [Test]
    public void Validate_EmailLocalPartTooLong_ReturnsValidationError()
    {
        //ARRANGE
        var email = $"{new string('a', 65)}@example.com";

        var validator = new UserRegisterRequestValidator(_logger, _usernameValidatorOptions, _timeProvider);

        //ACT
        var result = validator.TestValidate(CreateRequest(email: email));

        //ASSERT
        result.ShouldHaveValidationErrorFor(x => x.Body.Email)
            .WithErrorCode(ErrorCodes.UserAuth.InvalidEmail);
    }

    [Test]
    [TestCase("", ErrorCodes.UserAuth.InvalidPassword)]
    [TestCase("short", ErrorCodes.UserAuth.PasswordIsTooShort)]
    [TestCase("this-password-is-too-long", ErrorCodes.UserAuth.PasswordIsTooLong)]
    public void Validate_InvalidPassword_ReturnsValidationError(string password, string errorCode)
    {
        //ARRANGE
        var validator = new UserRegisterRequestValidator(_logger, _usernameValidatorOptions, _timeProvider);

        //ACT
        var result = validator.TestValidate(CreateRequest(password: password));

        //ASSERT
        result.ShouldHaveValidationErrorFor(x => x.Body.Password)
            .WithErrorCode(errorCode);
    }

    [TestCase("short-secret")]
    [TestCase("this-password-is-too-long-secret")]
    public void Validate_InvalidPassword_DoesNotLogPassword(string password)
    {
        //ARRANGE
        var validator = new UserRegisterRequestValidator(_logger, _usernameValidatorOptions, _timeProvider);

        //ACT
        validator.TestValidate(CreateRequest(password: password));

        //ASSERT
        _logger.NotReceivedLog(LogLevel.Information, message => message.Contains(password));
    }

    [Test]
    public void Validate_DateOfBirthInFuture_ReturnsValidationError()
    {
        //ARRANGE
        var tomorrow = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime).AddDays(1);
        var validator = new UserRegisterRequestValidator(_logger, _usernameValidatorOptions, _timeProvider);

        //ACT
        var result = validator.TestValidate(CreateRequest(dateOfBirth: tomorrow));

        //ASSERT
        result.ShouldHaveValidationErrorFor(x => x.Body.DateOfBirth)
            .WithErrorCode(ErrorCodes.UserAuth.InvalidDateOfBirth);
    }

    [Test]
    public void Validate_UnderMinimumAge_ReturnsValidationError()
    {
        //ARRANGE
        var validator = new UserRegisterRequestValidator(_logger, _usernameValidatorOptions, _timeProvider);

        //ACT
        var result = validator.TestValidate(CreateRequest(dateOfBirth: new DateOnly(2009, 1, 2)));

        //ASSERT
        result.ShouldHaveValidationErrorFor(x => x.Body.DateOfBirth)
            .WithErrorCode(ErrorCodes.UserAuth.UnderMinimumAge);
    }

    [Test]
    public void Validate_ExceedsMaximumAge_ReturnsValidationError()
    {
        //ARRANGE
        var validator = new UserRegisterRequestValidator(_logger, _usernameValidatorOptions, _timeProvider);

        //ACT
        var result = validator.TestValidate(CreateRequest(dateOfBirth: new DateOnly(1905, 1, 1)));

        //ASSERT
        result.ShouldHaveValidationErrorFor(x => x.Body.DateOfBirth)
            .WithErrorCode(ErrorCodes.UserAuth.ExceedsMaximumAge);
    }

    private static UserRegisterRequest CreateRequest(
        string username = "validuser",
        string email = "user@example.com",
        string password = "valid-password",
        DateOnly? dateOfBirth = null)
    {
        return new UserRegisterRequest
        {
            Body = new AuthUserRegisterRequestBody(
                username,
                email,
                password,
                dateOfBirth ?? DateOnly.FromDateTime((DateTimeOffset.UtcNow - TimeSpan.FromDays(19 * 365)).DateTime)
            )
        };
    }
}
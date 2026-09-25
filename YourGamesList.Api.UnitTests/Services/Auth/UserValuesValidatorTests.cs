using System;
using System.Linq;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Api.Services.Auth.Model;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.Auth;

public class UserValuesValidatorTests
{
    private IFixture _fixture;
    private ILogger<UserValuesValidator> _logger;
    private IOptions<UserValuesValidatorOptions> _options;
    private FakeTimeProvider _timeProvider;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<UserValuesValidator>>();
        _options = Substitute.For<IOptions<UserValuesValidatorOptions>>();
        _timeProvider = new FakeTimeProvider();
    }

    #region Email

    [Test]
    public void ValidateEmail_ValidEmail_ReturnsTrue()
    {
        //ARRANGE
        var options = _fixture.Build<UserValuesValidatorOptions>()
            .With(x => x.Email, _fixture.Build<EmailValidatorOptions>()
                .With(x => x.MaximumEmailLength, 256)
                .Create()).Create();
        _options.Value.Returns(options);
        const string email = "thisis@correct.email.com";
        var usernameValidator = new UserValuesValidator(_logger, _options, _timeProvider);

        //ACT
        var res = usernameValidator.ValidateEmail(email);

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
    }

    [Test]
    public void ValidateEmail_EmailTooLong_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<UserValuesValidatorOptions>()
            .With(x => x.Email, _fixture.Build<EmailValidatorOptions>()
                .With(x => x.MaximumEmailLength, 3)
                .Create()).Create();
        _options.Value.Returns(options);
        const string email = "thisis@incorrect.email.com";
        var usernameValidator = new UserValuesValidator(_logger, _options, _timeProvider);

        //ACT
        var res = usernameValidator.ValidateEmail(email);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.EmailIsTooLong));
        _logger.ReceivedLog(LogLevel.Information, $"email length {email.Length} is more than {options.Email.MaximumEmailLength} allowed.");
    }

    [Test]
    public void ValidateEmail_LocalPartTooLong_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<UserValuesValidatorOptions>()
            .With(x => x.Email, _fixture.Build<EmailValidatorOptions>()
                .With(x => x.MaximumEmailLength, 256)
                .Create()).Create();
        _options.Value.Returns(options);
        var localPart = new string(Enumerable.Range(0, 65).Select(_ => 'x').ToArray());
        var email = $"{localPart}@incorrect.email.com";
        var usernameValidator = new UserValuesValidator(_logger, _options, _timeProvider);

        //ACT
        var res = usernameValidator.ValidateEmail(email);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.InvalidEmail));
        _logger.ReceivedLog(LogLevel.Information, $"{localPart} is invalid email local part.");
    }

    #endregion

    #region Username

    [Test]
    public void ValidateUsername_ValidUsername_ReturnsTrue()
    {
        //ARRANGE
        var options = _fixture.Build<UserValuesValidatorOptions>()
            .With(x => x.Username, _fixture.Build<UsernameValidatorOptions>()
                .With(x => x.AllowedCharactersRegex, @"[\s\S]")
                .With(x => x.MinimumUsernameLength, 1)
                .With(x => x.MaximumUsernameLength, 100)
                .Create()).Create();
        _options.Value.Returns(options);
        const string username = "this_is_correct_Username";
        var usernameValidator = new UserValuesValidator(_logger, _options, _timeProvider);

        //ACT
        var res = usernameValidator.ValidateUsername(username);

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
    }

    [Test]
    public void ValidateUsername_UsernameTooShort_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<UserValuesValidatorOptions>()
            .With(x => x.Username, _fixture.Build<UsernameValidatorOptions>()
                .With(x => x.MinimumUsernameLength, 90)
                .With(x => x.MaximumUsernameLength, 100)
                .Create()).Create();
        _options.Value.Returns(options);
        const string username = "this_is_incorrect_username";
        var usernameValidator = new UserValuesValidator(_logger, _options, _timeProvider);

        //ACT
        var res = usernameValidator.ValidateUsername(username);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.UsernameIsTooShort));
        _logger.ReceivedLog(LogLevel.Information, $"username length {username.Length} is less than {options.Username.MinimumUsernameLength} required.");
    }

    [Test]
    public void ValidateUsername_UsernameTooLong_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<UserValuesValidatorOptions>()
            .With(x => x.Username, _fixture.Build<UsernameValidatorOptions>()
                .With(x => x.MinimumUsernameLength, 1)
                .With(x => x.MaximumUsernameLength, 2)
                .Create()).Create();
        _options.Value.Returns(options);
        const string username = "this_is_incorrect_username";
        var usernameValidator = new UserValuesValidator(_logger, _options, _timeProvider);

        //ACT
        var res = usernameValidator.ValidateUsername(username);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.UsernameIsTooLong));
        _logger.ReceivedLog(LogLevel.Information, $"username length {username.Length} is more than {options.Username.MaximumUsernameLength} allowed.");
    }


    [Test]
    public void ValidateUsername_InvalidCharacters_ReturnsTrue()
    {
        //ARRANGE
        var options = _fixture.Build<UserValuesValidatorOptions>()
            .With(x => x.Username, _fixture.Build<UsernameValidatorOptions>()
                .With(x => x.AllowedCharactersRegex, "^[a-zA-Z]+$")
                .With(x => x.MinimumUsernameLength, 1)
                .With(x => x.MaximumUsernameLength, 100)
                .Create()).Create();
        _options.Value.Returns(options);
        const string username = "this_is_incorrect_because_contains_numbers_1234";
        var usernameValidator = new UserValuesValidator(_logger, _options, _timeProvider);

        //ACT
        var res = usernameValidator.ValidateUsername(username);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.InvalidUsername));
        _logger.ReceivedLog(LogLevel.Information, $"username doesn't match allowed characters.");
    }

    #endregion

    #region Password

    [Test]
    public void ValidatePassword_ValidPassword_ReturnsTrue()
    {
        //ARRANGE
        var options = _fixture.Build<UserValuesValidatorOptions>()
            .With(x => x.Password, _fixture.Build<PasswordValidatorOptions>()
                .With(x => x.MinimumPasswordLength, 1)
                .With(x => x.MaximumPasswordLength, 100)
                .Create()).Create();
        _options.Value.Returns(options);
        const string password = "this_is_correct_password";
        var passwordValidator = new UserValuesValidator(_logger, _options, _timeProvider);

        //ACT
        var res = passwordValidator.ValidatePassword(password);

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
    }

    [Test]
    public void ValidatePassword_PasswordTooShort_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<UserValuesValidatorOptions>()
            .With(x => x.Password, _fixture.Build<PasswordValidatorOptions>()
                .With(x => x.MinimumPasswordLength, 90)
                .With(x => x.MaximumPasswordLength, 100)
                .Create()).Create();
        _options.Value.Returns(options);
        const string password = "this_is_incorrect_password";
        var passwordValidator = new UserValuesValidator(_logger, _options, _timeProvider);

        //ACT
        var res = passwordValidator.ValidatePassword(password);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.PasswordIsTooShort));
        _logger.ReceivedLog(LogLevel.Information, $"password length {password.Length} is less than {options.Password.MinimumPasswordLength} required.");
    }

    [Test]
    public void ValidatePassword_PasswordTooLong_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<UserValuesValidatorOptions>()
            .With(x => x.Password, _fixture.Build<PasswordValidatorOptions>()
                .With(x => x.MinimumPasswordLength, 1)
                .With(x => x.MaximumPasswordLength, 2)
                .Create()).Create();
        _options.Value.Returns(options);
        const string password = "this_is_incorrect_password";
        var passwordValidator = new UserValuesValidator(_logger, _options, _timeProvider);

        //ACT
        var res = passwordValidator.ValidatePassword(password);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.PasswordIsTooLong));
        _logger.ReceivedLog(LogLevel.Information, $"password length {password.Length} is more than {options.Password.MaximumPasswordLength} allowed.");
    }

    #endregion

    #region Date Of Birth

    [Test]
    public void ValidateDateOfBirth_ValidDateOfBirth_ReturnsTrue()
    {
        //ARRANGE
        var today = DateTimeOffset.UtcNow;
        _timeProvider.SetUtcNow(today);

        var options = _fixture.Build<UserValuesValidatorOptions>()
            .With(x => x.DateOfBirth, _fixture.Build<DateOfBirthValidatorOptions>()
                .With(x => x.MinimumAge, 18)
                .With(x => x.MaximumAge, 100)
                .Create()).Create();
        _options.Value.Returns(options);

        var dateOfBirth = DateOnly.FromDateTime(today.DateTime.AddYears(-20));
        var validator = new UserValuesValidator(_logger, _options, _timeProvider);

        //ACT
        var res = validator.ValidateDateOfBirth(dateOfBirth);

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
    }

    [Test]
    public void ValidateDateOfBirth_DateInFuture_ReturnsFalse()
    {
        //ARRANGE
        var today = DateTimeOffset.UtcNow;
        _timeProvider.SetUtcNow(today);

        var options = _fixture.Build<UserValuesValidatorOptions>()
            .With(x => x.DateOfBirth, _fixture.Build<DateOfBirthValidatorOptions>()
                .With(x => x.MinimumAge, 18)
                .With(x => x.MaximumAge, 100)
                .Create()).Create();
        _options.Value.Returns(options);

        var dateOfBirth = DateOnly.FromDateTime(today.DateTime.AddDays(1));
        var validator = new UserValuesValidator(_logger, _options, _timeProvider);

        //ACT
        var res = validator.ValidateDateOfBirth(dateOfBirth);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.InvalidDateOfBirth));
        _logger.ReceivedLog(LogLevel.Information, "date of birth is in the future.");
    }

    [Test]
    public void ValidateDateOfBirth_LesserThanMinimumAge_ReturnsFalse()
    {
        //ARRANGE
        var today = DateTimeOffset.UtcNow;
        _timeProvider.SetUtcNow(today);

        var options = _fixture.Build<UserValuesValidatorOptions>()
            .With(x => x.DateOfBirth, _fixture.Build<DateOfBirthValidatorOptions>()
                .With(x => x.MinimumAge, 18)
                .With(x => x.MaximumAge, 100)
                .Create()).Create();
        _options.Value.Returns(options);

        var dateOfBirth = DateOnly.FromDateTime(today.DateTime.AddYears(-17)); // 17 years old
        var validator = new UserValuesValidator(_logger, _options, _timeProvider);

        //ACT
        var res = validator.ValidateDateOfBirth(dateOfBirth);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.UnderMinimumAge));
        _logger.ReceivedLog(LogLevel.Information, "date of birth does not meet minimum age restrictions.");
    }

    [Test]
    public void ValidateDateOfBirth_MoreThanMaximumAge_ReturnsFalse()
    {
        //ARRANGE
        var today = DateTimeOffset.UtcNow;
        _timeProvider.SetUtcNow(today);

        var options = _fixture.Build<UserValuesValidatorOptions>()
            .With(x => x.DateOfBirth, _fixture.Build<DateOfBirthValidatorOptions>()
                .With(x => x.MinimumAge, 18)
                .With(x => x.MaximumAge, 100)
                .Create()).Create();
        _options.Value.Returns(options);

        var dateOfBirth = DateOnly.FromDateTime(today.DateTime.AddYears(-101)); // 101 years old
        var validator = new UserValuesValidator(_logger, _options, _timeProvider);

        //ACT
        var res = validator.ValidateDateOfBirth(dateOfBirth);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.ExceedsMaximumAge));
        _logger.ReceivedLog(LogLevel.Information, "date of birth exceeds maximum age restrictions.");
    }

    #endregion
}
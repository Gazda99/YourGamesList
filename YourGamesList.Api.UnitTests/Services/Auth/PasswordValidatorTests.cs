using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Api.Services.Auth.Model;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.Auth;

public class PasswordValidatorTests
{
    private IFixture _fixture;
    private ILogger<PasswordValidator> _logger;
    private IOptions<PasswordValidatorOptions> _options;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<PasswordValidator>>();
        _options = Substitute.For<IOptions<PasswordValidatorOptions>>();
    }

    [Test]
    public void ValidatePassword_ValidPassword_ReturnsTrue()
    {
        //ARRANGE
        var options = _fixture.Build<PasswordValidatorOptions>()
            .With(x => x.MinimumPasswordLength, 1)
            .With(x => x.MaximumPasswordLength, 100)
            .Create();
        _options.Value.Returns(options);
        const string password = "this_is_correct_password";
        var passwordValidator = new PasswordValidator(_logger, _options);

        //ACT
        var res = passwordValidator.ValidatePassword(password);

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
    }
    
    [Test]
    public void ValidatePassword_PasswordTooShort_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<PasswordValidatorOptions>()
            .With(x => x.MinimumPasswordLength, 90)
            .With(x => x.MaximumPasswordLength, 100)
            .Create();
        _options.Value.Returns(options);
        const string password = "this_is_incorrect_password";
        var passwordValidator = new PasswordValidator(_logger, _options);

        //ACT
        var res = passwordValidator.ValidatePassword(password);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.PasswordIsTooShort));
        _logger.ReceivedLog(LogLevel.Information, $"Password length '{password.Length}' is less than '{options.MinimumPasswordLength}' required.");
    }
    
    [Test]
    public void ValidatePassword_PasswordTooLong_ReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Build<PasswordValidatorOptions>()
            .With(x => x.MinimumPasswordLength, 1)
            .With(x => x.MaximumPasswordLength, 2)
            .Create();
        _options.Value.Returns(options);
        const string password = "this_is_incorrect_password";
        var passwordValidator = new PasswordValidator(_logger, _options);

        //ACT
        var res = passwordValidator.ValidatePassword(password);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.PasswordIsTooLong));
        _logger.ReceivedLog(LogLevel.Information, $"Password length '{password.Length}' is more than '{options.MaximumPasswordLength}' allowed.");
    }
}
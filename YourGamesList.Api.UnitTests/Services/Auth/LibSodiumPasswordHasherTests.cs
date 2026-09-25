using AutoFixture;
using LibGazdium;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.Auth;

public class LibSodiumPasswordHasherTests
{
    private IFixture _fixture;
    private ILogger<LibSodiumPasswordHasher> _logger;
    private IOptions<LibSodiumPasswordHashingOptions> _iOptions;
    private ISodiumInitializer _sodiumInitializer;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<LibSodiumPasswordHasher>>();
        _iOptions = Substitute.For<IOptions<LibSodiumPasswordHashingOptions>>();
        _sodiumInitializer = Substitute.For<ISodiumInitializer>();
    }

    #region Initialization

    [Test]
    public void LibsodiumInitialized_OnStartup()
    {
        // Libsodium should be initialized only once!

        //ASSERT
        var options = new LibSodiumPasswordHashingOptions();
        _iOptions.Value.Returns(options);

        //ACT
        var hasher = new LibSodiumPasswordHasher(_logger, _iOptions, _sodiumInitializer);

        //ASSERT
        _sodiumInitializer.Received(1).EnsureInitialized();
    }

    #endregion

    #region HashPassword

    [Test]
    public void HashPassword_SuccessfulScenario_ReturnsHashedPassword()
    {
        //ARRANGE
        var options = new LibSodiumPasswordHashingOptions();
        _iOptions.Value.Returns(options);

        var password = _fixture.Create<string>();

        var hasher = new LibSodiumPasswordHasher(_logger, _iOptions, _sodiumInitializer);

        //ACT
        var hashedPassword = hasher.HashPassword(password);

        //ASSERT
        Assert.That(hashedPassword.IsSuccess, Is.True);
        Assert.That(hashedPassword.Value, Is.Not.Empty);
        _logger.NotReceivedLog(LogLevel.Warning, _ => true);
    }

    [Test]
    public void HashPassword_LibSodiumWrapperExceptionScenario_ReturnsFailure()
    {
        //ARRANGE
        var options = new LibSodiumPasswordHashingOptions() { MemoryLimit = 0 };
        _iOptions.Value.Returns(options);

        var password = _fixture.Create<string>();

        var hasher = new LibSodiumPasswordHasher(_logger, _iOptions, _sodiumInitializer);

        //ACT
        var hashedPassword = hasher.HashPassword(password);

        //ASSERT
        Assert.That(hashedPassword.IsSuccess, Is.False);
        _logger.ReceivedLog(LogLevel.Warning, ["LibGazdium throws an exception"]);
    }

    #endregion

    #region VerifyHashedPassword

    [Test]
    public void VerifyHashedPassword_SuccessfulScenario_ReturnsTrue()
    {
        //ARRANGE
        var options = new LibSodiumPasswordHashingOptions();
        _iOptions.Value.Returns(options);

        var password = _fixture.Create<string>();
        var hasher = new LibSodiumPasswordHasher(_logger, _iOptions, _sodiumInitializer);
        var hashedPassword = LibsodiumWrapper.CryptoPwhashStr(password, options.OpsLimit, options.MemoryLimit);

        //ACT
        var result = hasher.VerifyHashedPassword(hashedPassword, password);

        //ASSERT
        Assert.That(result, Is.True);
        _logger.ReceivedLog(LogLevel.Information, "Password has been verified.");
    }

    [Test]
    public void VerifyHashedPassword_FailureScenario_ReturnsFalse()
    {
        //ARRANGE
        var options = new LibSodiumPasswordHashingOptions();
        _iOptions.Value.Returns(options);

        var password = _fixture.Create<string>();
        var password2 = _fixture.Create<string>();
        var hasher = new LibSodiumPasswordHasher(_logger, _iOptions, _sodiumInitializer);
        var hashedPassword = LibsodiumWrapper.CryptoPwhashStr(password, options.OpsLimit, options.MemoryLimit);

        //ACT
        var result = hasher.VerifyHashedPassword(hashedPassword, password2);

        //ASSERT
        Assert.That(result, Is.False);
        _logger.ReceivedLog(LogLevel.Information, "Password verification failed.");
    }

    #endregion
}
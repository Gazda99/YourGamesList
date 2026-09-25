using System;
using System.Threading.Tasks;
using AutoFixture;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using YourGamesList.Api.Services;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.Auth;

public class VerificationEmailSenderTests
{
    private IFixture _fixture;
    private ILogger<VerificationEmailSender> _logger;
    private IOptions<VerificationEmailSenderOptions> _options;
    private ISmtpClientFactory _smtpClientFactory;
    private ISmtpClient _smtpClient;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<VerificationEmailSender>>();
        _options = Substitute.For<IOptions<VerificationEmailSenderOptions>>();
        _smtpClientFactory = Substitute.For<ISmtpClientFactory>();
        _smtpClient = Substitute.For<ISmtpClient>();

        _smtpClientFactory.CreateClient().Returns(_smtpClient);
    }

    #region SendVerificationEmail

    [Test]
    public async Task SendVerificationEmail_ValidParameters_SendsEmailAndReturnsTrue()
    {
        //ARRANGE
        var options = _fixture.Build<VerificationEmailSenderOptions>()
            .With(x => x.YglUrl, "https://yourgameslist.com/")
            .With(x => x.Address, "noreply@yourgameslist.com")
            .With(x => x.SmtpHost, "smtp.yourgameslist.com")
            .With(x => x.SmtpPort, 587)
            .With(x => x.Username, "smtp_user")
            .With(x => x.Password, "smtp_pass")
            .Create();
        _options.Value.Returns(options);

        const string email = "user@example.com";
        const string username = "TestUser";
        const string token = "sample-verification-token";

        var sender = new VerificationEmailSender(_logger, _options, _smtpClientFactory);

        //ACT
        var result = await sender.SendVerificationEmail(email, username, token);

        //ASSERT
        Assert.That(result, Is.True);
        await _smtpClient.Received(1).ConnectAsync(options.SmtpHost, options.SmtpPort, true);
        await _smtpClient.Received(1).AuthenticateAsync(options.Username, options.Password);
        await _smtpClient.Received(1).SendAsync(Arg.Any<MimeMessage>());
        await _smtpClient.Received(1).DisconnectAsync(true);
        _logger.ReceivedLog(LogLevel.Information, "Email with verification link successfully sent.");
    }

    [Test]
    public async Task SendVerificationEmail_ValidParameters_ConstructsCorrectMimeMessage()
    {
        //ARRANGE
        var options = _fixture.Build<VerificationEmailSenderOptions>()
            .With(x => x.YglUrl, "https://yourgameslist.com/confirm")
            .With(x => x.Address, "noreply@yourgameslist.com")
            .Create();
        _options.Value.Returns(options);

        const string email = "user@example.com";
        const string username = "TestUser";
        const string token = "token+with/special=chars";

        MimeMessage capturedMessage = null;
        await _smtpClient.SendAsync(Arg.Do<MimeMessage>(msg => capturedMessage = msg));

        var sender = new VerificationEmailSender(_logger, _options, _smtpClientFactory);

        //ACT
        var result = await sender.SendVerificationEmail(email, username, token);

        //ASSERT
        Assert.That(result, Is.True);
        Assert.That(capturedMessage, Is.Not.Null);
        Assert.That(capturedMessage.Subject, Is.EqualTo($"Hello {username}, confirm your email address for newly created account in Your Games List"));
        Assert.That(capturedMessage.From.ToString(), Is.EqualTo($"\"Your Games List - automatic email service\" <{options.Address}>"));
        Assert.That(capturedMessage.To.ToString(), Is.EqualTo($"\"{username}\" <{email}>"));

        var expectedEscapedToken = Uri.EscapeDataString(token);
        var messageBody = capturedMessage.TextBody;
        Assert.That(messageBody, Does.Contain($"https://yourgameslist.com/confirm?token={expectedEscapedToken}"));
    }

    [Test]
    public async Task SendVerificationEmail_SmtpConnectThrowsException_LogsErrorAndReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Create<VerificationEmailSenderOptions>();
        _options.Value.Returns(options);

        _smtpClient.ConnectAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<bool>())
            .ThrowsAsync(new InvalidOperationException("SMTP server connection failed"));

        var sender = new VerificationEmailSender(_logger, _options, _smtpClientFactory);

        //ACT
        var result = await sender.SendVerificationEmail("user@example.com", "TestUser", "token123");

        //ASSERT
        Assert.That(result, Is.False);
        _logger.ReceivedLog(LogLevel.Error, "Email with verification link sending failed due to exception!");
    }

    [Test]
    public async Task SendVerificationEmail_SmtpSendThrowsException_LogsErrorAndReturnsFalse()
    {
        //ARRANGE
        var options = _fixture.Create<VerificationEmailSenderOptions>();
        _options.Value.Returns(options);

        _smtpClient.SendAsync(Arg.Any<MimeMessage>())
            .ThrowsAsync(new InvalidOperationException("Sending failed"));

        var sender = new VerificationEmailSender(_logger, _options, _smtpClientFactory);

        //ACT
        var result = await sender.SendVerificationEmail("user@example.com", "TestUser", "token123");

        //ASSERT
        Assert.That(result, Is.False);
        _logger.ReceivedLog(LogLevel.Error, "Email with verification link sending failed due to exception!");
    }

    #endregion
}
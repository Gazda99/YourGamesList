using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using YourGamesList.Api.Services.Auth.Options;

namespace YourGamesList.Api.Services.Auth;
//TODO: unit tests
public interface IVerificationEmailSender
{
    Task<bool> SendVerificationEmail(string email, string username, string token);
}

public class VerificationEmailSender : IVerificationEmailSender
{
    private readonly ILogger<VerificationEmailSender> _logger;
    private readonly IOptions<VerificationEmailSenderOptions> _options;

    public VerificationEmailSender(ILogger<VerificationEmailSender> logger, IOptions<VerificationEmailSenderOptions> options)
    {
        _logger = logger;
        _options = options;
    }

    private static string Body(string url, string token)
    {
        var confirmationUrl = $"{url.TrimEnd('/')}?token={Uri.EscapeDataString(token)}";

        return $"""
                Hi,

                Thanks for signing up for Your Games List!

                Please confirm your email address by visiting the following link:
                {confirmationUrl}

                If you didn't create an account, you can safely ignore this email.

                Best regards,
                Your Games List Team
                """;
    }

    public async Task<bool> SendVerificationEmail(string email, string username, string token)
    {
        const string senderName = "Your Games List - automatic email service";
        var subject = $"Hello {username}, confirm your email address for newly created account in Your Games List";
        var body = new TextPart("plain") { Text = Body(_options.Value.YglUrl, token) };

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, _options.Value.Address));
            message.To.Add(new MailboxAddress(username, email));
            message.Subject = subject;

            if (body != null)
            {
                message.Body = body;
            }

            using var client = new SmtpClient();
            await client.ConnectAsync(_options.Value.SmtpHost, _options.Value.SmtpPort, true);
            await client.AuthenticateAsync(_options.Value.Username, _options.Value.Password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email with verification link sending failed due to exception!");

            return false;
        }

        _logger.LogInformation("Email with verification link successfully sent.");
        return true;
    }
}
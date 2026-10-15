using MailKit.Net.Smtp;

namespace YourGamesList.Api.Services;

public interface ISmtpClientFactory
{
    /// <summary>
    /// Creates an SMTP client.
    /// </summary>
    ISmtpClient CreateClient();
}

public class SmtpClientFactory : ISmtpClientFactory
{
    public ISmtpClient CreateClient()
    {
        return new SmtpClient();
    }
}
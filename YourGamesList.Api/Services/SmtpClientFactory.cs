using MailKit.Net.Smtp;

namespace YourGamesList.Api.Services;

public interface ISmtpClientFactory
{
    ISmtpClient CreateClient();
}

public class SmtpClientFactory : ISmtpClientFactory
{
    public ISmtpClient CreateClient()
    {
        return new SmtpClient();
    }
}
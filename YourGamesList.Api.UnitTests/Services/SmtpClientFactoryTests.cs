using MailKit.Net.Smtp;
using YourGamesList.Api.Services;

namespace YourGamesList.Api.UnitTests.Services;

public class SmtpClientFactoryTests
{
    [Test]
    public void CreateSmtpClient_ReturnsSmtpClient()
    {
        //ARRANGE
        var factory = new SmtpClientFactory();
        
        //ACT
        var smtpClient = factory.CreateClient();
        
        //ASSERT
        Assert.NotNull(smtpClient);
        Assert.IsInstanceOf<SmtpClient>(smtpClient);
    }
}
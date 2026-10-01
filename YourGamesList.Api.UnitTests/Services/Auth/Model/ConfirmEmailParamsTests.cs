using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Api.Services.Auth.Model;

namespace YourGamesList.Api.UnitTests.Services.Auth.Model;

public class ConfirmEmailParamsTests
{
    [Test]
    public void ToParams_MapsToken()
    {
        var request = new ConfirmEmailRequest { Token = "verification-token" };

        var result = request.ToParams();

        Assert.That(result.Token, Is.EqualTo("verification-token"));
    }
}

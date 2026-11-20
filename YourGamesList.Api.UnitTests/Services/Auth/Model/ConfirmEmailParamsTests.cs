using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Api.Services.Auth.Model;
using YourGamesList.Contracts.Requests.Users;

namespace YourGamesList.Api.UnitTests.Services.Auth.Model;

public class ConfirmEmailParamsTests
{
    [Test]
    public void ToParams_MapsTrimmedToken()
    {
        var request = new ConfirmEmailRequest { Body = new AuthConfirmEmailRequestBody("  verification-token ") };

        var result = request.ToParams();

        Assert.That(result.Token, Is.EqualTo("verification-token"));
    }
}

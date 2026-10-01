using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Api.Services.Auth.Model;
using YourGamesList.Contracts.Requests.Users;

namespace YourGamesList.Api.UnitTests.Services.Auth.Model;

public class UserLoginParamsTests
{
    [Test]
    public void ToParams_MapsAllFields()
    {
        var request = new UserLoginRequest
        {
            Body = new AuthUserLoginRequestBody("testuser", "password")
        };

        var result = request.ToParams();

        Assert.That(result.Username, Is.EqualTo("testuser"));
        Assert.That(result.Password, Is.EqualTo("password"));
    }
}

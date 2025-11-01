using System.Net;
using System.Threading.Tasks;
using YourGamesList.IntegrationTests.Requests.Auth;
using YourGamesList.IntegrationTests.Utility;

namespace YourGamesList.IntegrationTests.Tests;

[TestFixture]
public class AuthTests
{
    private YglApi _yglApi;
    private string _userName;
    private string _userPassword;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _userName = Configuration.OneTimeUserName;
        _userPassword = Configuration.OneTimeUserPassword;

        _yglApi = new YglApi();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await TestLogger.Log("Starting one time teardown...");
        var deleteRequest = new UserDeleteRequest()
        {
            Username = _userName,
            Password = _userPassword
        };

        await TestLogger.Log($"Deleting user '{_userName}'...");
        var res = await _yglApi.Auth.Delete(deleteRequest);
        await TestLogger.LogApiResponse(res);
        await TestLogger.Log("One time teardown complete.");
    }

    [Test]
    [Order(10)]
    public async Task CreateLoginDeleteUser()
    {
        //---------------------------------------
        var registerRequest = new UserRegisterRequest()
        {
            Username = _userName,
            Password = _userPassword
        };

        await TestLogger.Log($"Creating user '{_userName}'...");
        var registerRes = await _yglApi.Auth.Register(registerRequest);
        await TestLogger.LogApiResponse(registerRes);
        Assert.That(registerRes.IsSuccessful, Is.True);
        Assert.That(registerRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        //---------------------------------------
        var loginRequest = new UserLoginRequest()
        {
            Username = _userName,
            Password = _userPassword
        };

        await TestLogger.Log($"Logging user '{_userName}'...");
        var loginRes = await _yglApi.Auth.Login(loginRequest);
        await TestLogger.LogApiResponse(loginRes);

        Assert.That(loginRes.IsSuccessful, Is.True);
        Assert.That(loginRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(loginRes.Content, Is.Not.Empty);

        //---------------------------------------
        var deleteRequest = new UserDeleteRequest()
        {
            Username = _userName,
            Password = _userPassword
        };

        await TestLogger.Log($"Deleting user '{_userName}'...");
        var deleteRes = await _yglApi.Auth.Delete(deleteRequest);
        await TestLogger.LogApiResponse(deleteRes);

        Assert.That(deleteRes.IsSuccessful, Is.True);
        Assert.That(deleteRes.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        //---------------------------------------
    }
}
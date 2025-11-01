using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Controllers;
using YourGamesList.Api.Model.Requests.Auth;
using YourGamesList.Api.Model.Responses.Auth;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Api.Services.Auth.Model;
using YourGamesList.Common;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Controllers;

public class AuthControllerTests
{
    private IFixture _fixture;
    private ILogger<AuthController> _logger;
    private IUserManagerService _userManagerService;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<AuthController>>();
        _userManagerService = Substitute.For<IUserManagerService>();
    }

    #region Register

    [Test]
    public async Task Register_SuccessScenario()
    {
        //ARRANGE
        var userName = _fixture.Create<string>();
        var password = _fixture.Create<string>();
        var registerRequest = new UserRegisterRequest()
        {
            Username = userName,
            Password = password
        };
        _userManagerService.RegisterUser(userName, password).Returns(ErrorResult<UserAuthError>.Clear());
        var controller = new AuthController(_logger, _userManagerService);

        //ACT
        var res = await controller.Register(registerRequest);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        _logger.ReceivedLog(LogLevel.Information, $"Requested to register user '{userName}'");
        await _userManagerService.Received(1).RegisterUser(userName, password);
    }

    [Test]
    public async Task Register_OnRegisterNameAlreadyTaken_Returns409Conflict()
    {
        //ARRANGE
        var userName = _fixture.Create<string>();
        var password = _fixture.Create<string>();
        var registerRequest = new UserRegisterRequest()
        {
            Username = userName,
            Password = password
        };
        _userManagerService.RegisterUser(userName, password).Returns(ErrorResult<UserAuthError>.Failure(UserAuthError.RegisterNameAlreadyTaken));
        var controller = new AuthController(_logger, _userManagerService);

        //ACT
        var res = await controller.Register(registerRequest);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
        _logger.ReceivedLog(LogLevel.Information, $"Requested to register user '{userName}'");
        await _userManagerService.Received(1).RegisterUser(userName, password);
    }

    [Test]
    [TestCase(UserAuthError.PasswordIsTooShort)]
    [TestCase(UserAuthError.PasswordIsTooLong)]
    public async Task Register_OnRegisterNameAlreadyTaken_Returns409Conflict(UserAuthError passwordRelatedError)
    {
        //ARRANGE
        var userName = _fixture.Create<string>();
        var password = _fixture.Create<string>();
        var registerRequest = new UserRegisterRequest()
        {
            Username = userName,
            Password = password
        };
        var error = ErrorResult<UserAuthError>.Failure(passwordRelatedError);
        _userManagerService.RegisterUser(userName, password).Returns(error);
        var controller = new AuthController(_logger, _userManagerService);

        //ACT
        var res = await controller.Register(registerRequest);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        Assert.That(objectResult.Value, Is.EqualTo(error.Error.ToString()));
        _logger.ReceivedLog(LogLevel.Information, $"Requested to register user '{userName}'");
        await _userManagerService.Received(1).RegisterUser(userName, password);
    }

    #endregion

    #region Login

    [Test]
    public async Task Login_SuccessScenario()
    {
        //ARRANGE
        var token = _fixture.Create<string>();
        var userName = _fixture.Create<string>();
        var password = _fixture.Create<string>();
        var loginRequest = new UserLoginRequest()
        {
            Username = userName,
            Password = password
        };
        _userManagerService.Login(userName, password).Returns(CombinedResult<string, UserAuthError>.Success(token));
        var controller = new AuthController(_logger, _userManagerService);

        //ACT
        var res = await controller.Login(loginRequest);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(objectResult.Value, Is.TypeOf<AuthLoginResponse>());
        var authLoginResponse = (AuthLoginResponse) objectResult.Value;
        Assert.That(authLoginResponse.Token, Is.EqualTo(token));
        _logger.ReceivedLog(LogLevel.Information, $"Requested to login user '{userName}'");
        await _userManagerService.Received(1).Login(userName, password);
    }

    [Test]
    public async Task Login_OnNoUserFound_Returns404NotFound()
    {
        //ARRANGE
        var userName = _fixture.Create<string>();
        var password = _fixture.Create<string>();
        var loginRequest = new UserLoginRequest()
        {
            Username = userName,
            Password = password
        };
        _userManagerService.Login(userName, password).Returns(CombinedResult<string, UserAuthError>.Failure(UserAuthError.NoUserFound));
        var controller = new AuthController(_logger, _userManagerService);

        //ACT
        var res = await controller.Login(loginRequest);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        _logger.ReceivedLog(LogLevel.Information, $"Requested to login user '{userName}'");
        await _userManagerService.Received(1).Login(userName, password);
    }

    [Test]
    public async Task Login_OnWrongPassword_ReturnsStatus401Unauthorized()
    {
        //ARRANGE
        var userName = _fixture.Create<string>();
        var password = _fixture.Create<string>();
        var loginRequest = new UserLoginRequest()
        {
            Username = userName,
            Password = password
        };
        _userManagerService.Login(userName, password).Returns(CombinedResult<string, UserAuthError>.Failure(UserAuthError.WrongPassword));
        var controller = new AuthController(_logger, _userManagerService);

        //ACT
        var res = await controller.Login(loginRequest);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        _logger.ReceivedLog(LogLevel.Information, $"Requested to login user '{userName}'");
        await _userManagerService.Received(1).Login(userName, password);
    }

    #endregion

    #region Delete

    [Test]
    public async Task Delete_SuccessScenario()
    {
        //ARRANGE
        var userName = _fixture.Create<string>();
        var password = _fixture.Create<string>();
        var deleteRequest = new UserDeleteRequest()
        {
            Username = userName,
            Password = password
        };
        _userManagerService.Delete(userName, password).Returns(ErrorResult<UserAuthError>.Clear());
        var controller = new AuthController(_logger, _userManagerService);

        //ACT
        var res = await controller.Delete(deleteRequest);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status204NoContent));
        _logger.ReceivedLog(LogLevel.Information, $"Requested to delete user '{userName}'");
        await _userManagerService.Received(1).Delete(userName, password);
    }

    [Test]
    public async Task Delete_OnNoUserFound_Returns404NotFound()
    {
        //ARRANGE
        var userName = _fixture.Create<string>();
        var password = _fixture.Create<string>();
        var deleteRequest = new UserDeleteRequest()
        {
            Username = userName,
            Password = password
        };
        _userManagerService.Delete(userName, password).Returns(ErrorResult<UserAuthError>.Failure(UserAuthError.NoUserFound));
        var controller = new AuthController(_logger, _userManagerService);

        //ACT
        var res = await controller.Delete(deleteRequest);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        _logger.ReceivedLog(LogLevel.Information, $"Requested to delete user '{userName}'");
        await _userManagerService.Received(1).Delete(userName, password);
    }

    [Test]
    public async Task Delete_OnWrongPassword_ReturnsStatus401Unauthorized()
    {
        //ARRANGE
        var userName = _fixture.Create<string>();
        var password = _fixture.Create<string>();
        var deleteRequest = new UserDeleteRequest()
        {
            Username = userName,
            Password = password
        };
        _userManagerService.Delete(userName, password).Returns(ErrorResult<UserAuthError>.Failure(UserAuthError.WrongPassword));
        var controller = new AuthController(_logger, _userManagerService);

        //ACT
        var res = await controller.Delete(deleteRequest);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        _logger.ReceivedLog(LogLevel.Information, $"Requested to delete user '{userName}'");
        await _userManagerService.Received(1).Delete(userName, password);
    }

    #endregion
}
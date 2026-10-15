using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Controllers.Users;
using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Api.Services;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Api.Services.Auth.Model;
using YourGamesList.Common;
using YourGamesList.Contracts.Responses;
using YourGamesList.Contracts.Responses.Users;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Controllers.Users;

public class UserAuthControllerTests
{
    private IFixture _fixture;
    private ILogger<UserAuthController> _logger;
    private IUserManagerService _userManagerService;
    private IDomainErrorMapper _domainErrorMapper;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _fixture.Customize<DateOnly>(composer =>
            composer.FromFactory(() => DateOnly.FromDateTime(_fixture.Create<DateTime>())));
        _logger = Substitute.For<ILogger<UserAuthController>>();
        _userManagerService = Substitute.For<IUserManagerService>();
        _domainErrorMapper = Substitute.For<IDomainErrorMapper>();
    }

    #region Register

    [Test]
    public async Task Register_ValidRequest_ReturnsOkWithUserId()
    {
        //ARRANGE
        var request = _fixture.Create<UserRegisterRequest>();
        var userId = Guid.NewGuid();
        var serviceResult = CombinedResult<Guid, UserAuthError>.Success(userId);

        _userManagerService.Register(Arg.Any<UserRegisterParams>()).Returns(serviceResult);

        var controller = CreateController();

        //ACT
        var res = await controller.Register(request);

        //ASSERT
        var objectResult = res as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(objectResult.Value, Is.EqualTo(userId.ToString()));
        _logger.ReceivedLog(LogLevel.Information, $"Requested to register user {request.Body.Username}");
    }

    [TestCase(UserAuthError.UsernameAlreadyTaken)]
    [TestCase(UserAuthError.EmailAlreadyTaken)]
    public async Task Register_ConflictError_ReturnsConflict(UserAuthError authError)
    {
        //ARRANGE
        var request = _fixture.Create<UserRegisterRequest>();
        var serviceResult = CombinedResult<Guid, UserAuthError>.Failure(authError);
        var error = _fixture.Create<Error>();
        _domainErrorMapper.Map(authError).Returns(error);

        _userManagerService.Register(Arg.Any<UserRegisterParams>()).Returns(serviceResult);

        var controller = CreateController();

        //ACT
        var res = await controller.Register(request);

        //ASSERT
        var objectResult = res as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
        var errorResponse = objectResult.Value as ErrorResponse;
        Assert.That(errorResponse, Is.Not.Null);
        var errors = errorResponse.Errors.ToArray();
        Assert.That(errors, Has.Length.EqualTo(1));
        Assert.That(errors[0], Is.SameAs(error));
    }

    [Test]
    public async Task Register_UnexpectedError_ReturnsInternalServerError()
    {
        //ARRANGE
        var request = _fixture.Create<UserRegisterRequest>();
        var serviceResult = CombinedResult<Guid, UserAuthError>.Failure(UserAuthError.InternalServerError);

        _userManagerService.Register(Arg.Any<UserRegisterParams>()).Returns(serviceResult);

        var controller = CreateController();

        //ACT
        var res = await controller.Register(request);

        //ASSERT
        var statusCodeResult = res as IStatusCodeActionResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    #endregion

    #region Login

    [Test]
    public async Task Login_ValidCredentials_ReturnsOkWithAuthLoginResponse()
    {
        //ARRANGE
        var request = _fixture.Create<UserLoginRequest>();
        const string token = "sample_jwt_token";
        var serviceResult = CombinedResult<string, UserAuthError>.Success(token);

        _userManagerService.Login(Arg.Any<UserLoginParams>()).Returns(serviceResult);

        var controller = CreateController();

        //ACT
        var res = await controller.Login(request);

        //ASSERT
        var objectResult = res as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));

        var responseBody = objectResult.Value as AuthLoginResponse;
        Assert.That(responseBody, Is.Not.Null);
        Assert.That(responseBody.Token, Is.EqualTo(token));
        _logger.ReceivedLog(LogLevel.Information, $"Requested to login user {request.Body.Username}");
    }

    [Test]
    public async Task Login_NoUserFound_ReturnsNotFound()
    {
        //ARRANGE
        var request = _fixture.Create<UserLoginRequest>();
        var serviceResult = CombinedResult<string, UserAuthError>.Failure(UserAuthError.NoUserFound);

        _userManagerService.Login(Arg.Any<UserLoginParams>()).Returns(serviceResult);

        var controller = CreateController();

        //ACT
        var res = await controller.Login(request);

        //ASSERT
        var statusCodeResult = res as IStatusCodeActionResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
    }

    [Test]
    public async Task Login_UserNotActiveEmailActivation_ReturnsForbidden()
    {
        //ARRANGE
        var request = _fixture.Create<UserLoginRequest>();
        var serviceResult = CombinedResult<string, UserAuthError>.Failure(UserAuthError.UserNotActiveEmailActivation);
        var error = _fixture.Create<Error>();
        _domainErrorMapper.Map(UserAuthError.UserNotActiveEmailActivation).Returns(error);

        _userManagerService.Login(Arg.Any<UserLoginParams>()).Returns(serviceResult);

        var controller = CreateController();

        //ACT
        var res = await controller.Login(request);

        //ASSERT
        var objectResult = res as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        var errorResponse = objectResult.Value as ErrorResponse;
        Assert.That(errorResponse, Is.Not.Null);
        var errors = errorResponse.Errors.ToArray();
        Assert.That(errors, Has.Length.EqualTo(1));
        Assert.That(errors[0], Is.SameAs(error));
    }

    [Test]
    public async Task Login_UnauthorizedUser_ReturnsUnauthorized()
    {
        //ARRANGE
        var request = _fixture.Create<UserLoginRequest>();
        var serviceResult = CombinedResult<string, UserAuthError>.Failure(UserAuthError.UnauthorizedUser);

        _userManagerService.Login(Arg.Any<UserLoginParams>()).Returns(serviceResult);

        var controller = CreateController();

        //ACT
        var res = await controller.Login(request);

        //ASSERT
        var statusCodeResult = res as IStatusCodeActionResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
    }

    [Test]
    public async Task Login_UnexpectedError_ReturnsInternalServerError()
    {
        //ARRANGE
        var request = _fixture.Create<UserLoginRequest>();
        var serviceResult = CombinedResult<string, UserAuthError>.Failure(UserAuthError.InvalidEmail);

        _userManagerService.Login(Arg.Any<UserLoginParams>()).Returns(serviceResult);

        var controller = CreateController();

        //ACT
        var res = await controller.Login(request);

        //ASSERT
        var statusCodeResult = res as IStatusCodeActionResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    #endregion

    #region Delete

    [Test]
    public async Task Delete_ValidRequest_ReturnsNoContent()
    {
        //ARRANGE
        var request = _fixture.Create<UserDeleteRequest>();
        var serviceResult = ErrorResult<UserAuthError>.Success();

        _userManagerService.Delete(Arg.Any<UserDeleteParams>()).Returns(serviceResult);

        var controller = CreateController();

        //ACT
        var res = await controller.Delete(request);

        //ASSERT
        var statusCodeResult = res as IStatusCodeActionResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status204NoContent));
        _logger.ReceivedLog(LogLevel.Information, $"Requested to delete user {request.Body.Username}");
    }

    [Test]
    public async Task Delete_NoUserFound_ReturnsNotFound()
    {
        //ARRANGE
        var request = _fixture.Create<UserDeleteRequest>();
        var serviceResult = ErrorResult<UserAuthError>.Failure(UserAuthError.NoUserFound);

        _userManagerService.Delete(Arg.Any<UserDeleteParams>()).Returns(serviceResult);

        var controller = CreateController();

        //ACT
        var res = await controller.Delete(request);

        //ASSERT
        var statusCodeResult = res as IStatusCodeActionResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
    }

    [Test]
    public async Task Delete_UnauthorizedUser_ReturnsUnauthorized()
    {
        //ARRANGE
        var request = _fixture.Create<UserDeleteRequest>();
        var serviceResult = ErrorResult<UserAuthError>.Failure(UserAuthError.UnauthorizedUser);

        _userManagerService.Delete(Arg.Any<UserDeleteParams>()).Returns(serviceResult);

        var controller = CreateController();

        //ACT
        var res = await controller.Delete(request);

        //ASSERT
        var statusCodeResult = res as IStatusCodeActionResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
    }

    [Test]
    public async Task Delete_UnexpectedError_ReturnsInternalServerError()
    {
        //ARRANGE
        var request = _fixture.Create<UserDeleteRequest>();
        var serviceResult = ErrorResult<UserAuthError>.Failure(UserAuthError.InvalidEmail);

        _userManagerService.Delete(Arg.Any<UserDeleteParams>()).Returns(serviceResult);

        var controller = CreateController();

        //ACT
        var res = await controller.Delete(request);

        //ASSERT
        var statusCodeResult = res as IStatusCodeActionResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    #endregion

    #region ConfirmEmail

    [Test]
    public async Task ConfirmEmail_ValidToken_ReturnsOk()
    {
        //ARRANGE
        var request = _fixture.Create<ConfirmEmailRequest>();
        var serviceResult = ErrorResult<UserAuthError>.Success();

        _userManagerService.ConfirmEmail(Arg.Any<ConfirmEmailParams>()).Returns(serviceResult);

        var controller = CreateController();

        //ACT
        var res = await controller.ConfirmEmail(request);

        //ASSERT
        var statusCodeResult = res as IStatusCodeActionResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        _logger.ReceivedLog(LogLevel.Information, "Requested to confirm email");
    }

    [TestCase(UserAuthError.EmailVerificationInvalid)]
    [TestCase(UserAuthError.EmailVerificationExpired)]
    public async Task ConfirmEmail_InvalidOrExpiredToken_ReturnsBadRequest(UserAuthError authError)
    {
        //ARRANGE
        var request = _fixture.Create<ConfirmEmailRequest>();
        var serviceResult = ErrorResult<UserAuthError>.Failure(authError);
        var error = _fixture.Create<Error>();
        _domainErrorMapper.Map(authError).Returns(error);

        _userManagerService.ConfirmEmail(Arg.Any<ConfirmEmailParams>()).Returns(serviceResult);

        var controller = CreateController();

        //ACT
        var res = await controller.ConfirmEmail(request);

        //ASSERT
        var objectResult = res as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        var errorResponse = objectResult.Value as ErrorResponse;
        Assert.That(errorResponse, Is.Not.Null);
        var errors = errorResponse.Errors.ToArray();
        Assert.That(errors, Has.Length.EqualTo(1));
        Assert.That(errors[0], Is.SameAs(error));
    }

    [Test]
    public async Task ConfirmEmail_UnexpectedError_ReturnsInternalServerError()
    {
        //ARRANGE
        var request = _fixture.Create<ConfirmEmailRequest>();
        var serviceResult = ErrorResult<UserAuthError>.Failure(UserAuthError.NoUserFound);

        _userManagerService.ConfirmEmail(Arg.Any<ConfirmEmailParams>()).Returns(serviceResult);

        var controller = CreateController();

        //ACT
        var res = await controller.ConfirmEmail(request);

        //ASSERT
        var statusCodeResult = res as IStatusCodeActionResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    #endregion

    private UserAuthController CreateController()
    {
        return new UserAuthController(_logger, _userManagerService, _domainErrorMapper);
    }
}
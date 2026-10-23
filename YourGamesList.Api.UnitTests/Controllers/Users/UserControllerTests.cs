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
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Api.Services;
using YourGamesList.Api.Services.YourGamesList;
using YourGamesList.Api.Services.YourGamesList.Model;
using YourGamesList.Common;
using YourGamesList.Contracts.Dto;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.UnitTests.Controllers.Users;

public class UserControllerTests
{
    private IFixture _fixture = null!;
    private ILogger<UserController> _logger = null!;
    private IUsersService _usersService = null!;
    private IDomainErrorMapper _domainErrorMapper = null!;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<UserController>>();
        _usersService = Substitute.For<IUsersService>();
        _domainErrorMapper = Substitute.For<IDomainErrorMapper>();
    }

    [Test]
    public async Task GetSelfUser_UserFound_ReturnsOkWithUserAndPassesAuthenticatedUser()
    {
        var userInformation = new UserInformationToken
        {
            RawToken = "token",
            NormalizedUsername = "testuser",
            UserId = Guid.NewGuid()
        };
        var request = new UserGetSelfRequest { UserInformation = userInformation };
        var user = new UserDto
        {
            Id = userInformation.UserId,
            CreatedAt = DateTimeOffset.UtcNow,
            Username = userInformation.NormalizedUsername,
            Description = null,
            DateOfBirth = new DateOnly(2000, 1, 1)
        };
        _usersService.GetSelfUser(Arg.Any<UserGetSelfParams>())
            .Returns(CombinedResult<UserDto, UsersError>.Success(user));

        var controller = CreateController();

        var result = await controller.GetSelfUser(request);

        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(objectResult.Value, Is.SameAs(user));
        await _usersService.Received(1).GetSelfUser(
            Arg.Is<UserGetSelfParams>(parameters => parameters.UserInformation == userInformation));
    }

    [Test]
    public async Task GetSelfUser_UserNotFound_ReturnsNotFound()
    {
        var error = _fixture.Create<Error>();
        _domainErrorMapper.Map(UsersError.UserNotFound).Returns(error);
        var request = CreateRequest();
        _usersService.GetSelfUser(Arg.Any<UserGetSelfParams>())
            .Returns(CombinedResult<UserDto, UsersError>.Failure(UsersError.UserNotFound));

        var controller = CreateController();

        var result = await controller.GetSelfUser(request);

        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        var errorResponse = objectResult.Value as ErrorResponse;
        Assert.That(errorResponse, Is.Not.Null);
        var errors = errorResponse.Errors.ToArray();
        Assert.That(errors, Has.Length.EqualTo(1));
        Assert.That(errors[0], Is.SameAs(error));
    }

    [Test]
    public async Task GetSelfUser_UnexpectedError_ReturnsInternalServerError()
    {
        var request = CreateRequest();
        _usersService.GetSelfUser(Arg.Any<UserGetSelfParams>())
            .Returns(CombinedResult<UserDto, UsersError>.Failure((UsersError)int.MaxValue));

        var controller = CreateController();

        var result = await controller.GetSelfUser(request);

        Assert.That(result, Is.InstanceOf<IStatusCodeActionResult>());
        Assert.That(((IStatusCodeActionResult)result).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    private static UserGetSelfRequest CreateRequest()
    {
        return new UserGetSelfRequest
        {
            UserInformation = new UserInformationToken
            {
                RawToken = "token",
                NormalizedUsername = "testuser",
                UserId = Guid.NewGuid()
            }
        };
    }

    private UserController CreateController()
    {
        return new UserController(_logger, _usersService, _domainErrorMapper);
    }
}

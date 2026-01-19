using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Controllers;
using YourGamesList.Api.Controllers.Users;
using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Api.Services;
using YourGamesList.Api.Services.ModelMappers;
using YourGamesList.Api.Services.Users;
using YourGamesList.Api.Services.Users.Model;
using YourGamesList.Common;
using YourGamesList.Contracts.Dto;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Controllers;

public class UsersControllerTests
{
    private IFixture _fixture;
    private ILogger<UsersController> _logger;
    private IRequestToParametersMapper _requestToParametersMapper;
    private IUsersService _usersService;
    private ICountriesService _countriesService;


    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<UsersController>>();
        _requestToParametersMapper = Substitute.For<IRequestToParametersMapper>();
        _usersService = Substitute.For<IUsersService>();
        _countriesService = Substitute.For<ICountriesService>();
    }

    #region GetSelf

    [Test]
    public async Task GetSelf_SuccessScenario()
    {
        //ARRANGE
        var expectedResValue = _fixture.Create<UserDto>();
        var request = _fixture.Create<UserGetSelfRequest>();
        var parameters = _fixture.Create<UserGetSelfParameters>();
        _requestToParametersMapper.Map(request).Returns(parameters);
        _usersService.GetSelfUser(parameters).Returns(CombinedResult<UserDto, UsersError>.Success(expectedResValue));

        var controller = new UsersController(_logger, _requestToParametersMapper, _usersService, _countriesService);

        //ACT
        var res = await controller.GetSelf(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(objectResult.Value, Is.EqualTo(expectedResValue));
        _requestToParametersMapper.Received(1).Map(request);
        await _usersService.Received(1).GetSelfUser(parameters);
        _logger.ReceivedLog(LogLevel.Information, $"Requested to get self user '{request.UserInformation.UserId}'");
    }

    [Test]
    public async Task GetSelf_OnErrorUserNotFound_ReturnsStatus404NotFound()
    {
        //ARRANGE
        var request = _fixture.Create<UserGetSelfRequest>();
        var parameters = _fixture.Create<UserGetSelfParameters>();
        _requestToParametersMapper.Map(request).Returns(parameters);
        _usersService.GetSelfUser(parameters).Returns(CombinedResult<UserDto, UsersError>.Failure(UsersError.UserNotFound));

        var controller = new UsersController(_logger, _requestToParametersMapper, _usersService, _countriesService);

        //ACT
        var res = await controller.GetSelf(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        _requestToParametersMapper.Received(1).Map(request);
        await _usersService.Received(1).GetSelfUser(parameters);
    }

    #endregion

    #region UpdateUser

    [Test]
    public async Task UpdateUser_SuccessScenario()
    {
        //ARRANGE
        var expectedResValue = Guid.NewGuid();
        var request = _fixture.Create<UserUpdateRequest>();
        var parameters = _fixture.Create<UserUpdateParameters>();
        _requestToParametersMapper.Map(request).Returns(parameters);
        _usersService.UpdateUser(parameters).Returns(CombinedResult<Guid, UsersError>.Success(expectedResValue));

        var controller = new UsersController(_logger, _requestToParametersMapper, _usersService, _countriesService);

        //ACT
        var res = await controller.UpdateUser(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(objectResult.Value, Is.EqualTo(expectedResValue));
        _requestToParametersMapper.Received(1).Map(request);
        await _usersService.Received(1).UpdateUser(parameters);
        _logger.ReceivedLog(LogLevel.Information, $"Requested to update user '{request.UserInformation.UserId}'");
    }

    [Test]
    public async Task UpdateUser_OnErrorUserNotFound_ReturnsStatus404NotFound()
    {
        //ARRANGE
        var request = _fixture.Create<UserUpdateRequest>();
        var parameters = _fixture.Create<UserUpdateParameters>();
        _requestToParametersMapper.Map(request).Returns(parameters);
        _usersService.UpdateUser(parameters).Returns(CombinedResult<Guid, UsersError>.Failure(UsersError.UserNotFound));

        var controller = new UsersController(_logger, _requestToParametersMapper, _usersService, _countriesService);

        //ACT
        var res = await controller.UpdateUser(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        _requestToParametersMapper.Received(1).Map(request);
        await _usersService.Received(1).UpdateUser(parameters);
        _logger.ReceivedLog(LogLevel.Information, $"Requested to update user '{request.UserInformation.UserId}'");
    }

    #endregion
}
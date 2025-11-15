using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Dto;
using YourGamesList.Api.Services.ModelMappers;
using YourGamesList.Api.Services.Users;
using YourGamesList.Api.Services.Users.Model;
using YourGamesList.Database;
using YourGamesList.Database.Entities;
using YourGamesList.Database.TestUtils;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.Users;

public class UsersServiceTests
{
    private IFixture _fixture;
    private ILogger<UsersService> _logger;
    private IYglDatabaseAndDtoMapper _yglDatabaseAndDtoMapper;

    private TestYglDbContextBuilder _yglDbContextBuilder;
    private IDbContextFactory<YglDbContext> _dbContextFactory;

    private YglDbContext _yglDbContext;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<UsersService>>();
        _yglDatabaseAndDtoMapper = Substitute.For<IYglDatabaseAndDtoMapper>();

        _yglDbContextBuilder = TestYglDbContextBuilder.Build();
        _dbContextFactory = Substitute.For<IDbContextFactory<YglDbContext>>();
        _dbContextFactory.CreateDbContext().Returns(_yglDbContextBuilder.Get());
    }

    #region GetSelfUser

    [Test]
    public async Task GetSelfUser_SuccessfulScenario_ReturnsUserDto()
    {
        //ARRANGE
        var userId = Guid.NewGuid();
        var parameters = _fixture.Build<UserGetSelfParameters>()
            .With(x => x.UserInformation, _fixture.Build<JwtUserInformation>()
                .With(x => x.UserId, userId)
                .WithAutoProperties()
                .Create())
            .WithAutoProperties()
            .Create();
        var user = new User()
        {
            Id = userId,
            Username = parameters.UserInformation.Username,
            PasswordHash = _fixture.Create<string>(),
            CreatedDate = DateTime.UtcNow,
            Salt = _fixture.Create<byte[]>()
        };
        var users = new List<User>() { user };
        _yglDbContextBuilder.WithUserDbSet(users);
        var userDto = _fixture.Create<UserDto>();
        _yglDatabaseAndDtoMapper.Map(Arg.Is<User>(u => u.Id == userId)).Returns(userDto);

        var usersService = new UsersService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await usersService.GetSelfUser(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(userDto));
        _logger.ReceivedLog(LogLevel.Information, $"Retrieved user with ID '{userId}'");
    }

    [Test]
    public async Task GetSelfUser_UserDoesNotExists_ReturnsUserNotFoundError()
    {
        //ARRANGE
        var parameters = _fixture.Create<UserGetSelfParameters>();
        var usersService = new UsersService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await usersService.GetSelfUser(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(UsersError.UserNotFound));
        _logger.ReceivedLog(LogLevel.Warning, $"User with ID '{parameters.UserInformation.UserId}' not found.");
    }

    #endregion


    #region UpdateUser

    [Test]
    public async Task UpdateUser_SuccessfulScenario_UpdatesUserAndReturnsId()
    {
        //ARRANGE
        var userId = Guid.NewGuid();
        var parameters = _fixture.Build<UserUpdateParameters>()
            .With(x => x.UserInformation, _fixture.Build<JwtUserInformation>()
                .With(x => x.UserId, userId)
                .WithAutoProperties()
                .Create())
            .WithAutoProperties()
            .Create();
        var user = new User()
        {
            Id = userId,
            Username = parameters.UserInformation.Username,
            PasswordHash = _fixture.Create<string>(),
            CreatedDate = DateTime.UtcNow,
            Salt = _fixture.Create<byte[]>()
        };
        var users = new List<User>() { user };
        _yglDbContextBuilder.WithUserDbSet(users);
        var userDto = _fixture.Create<UserDto>();
        _yglDatabaseAndDtoMapper.Map(Arg.Is<User>(u => u.Id == userId)).Returns(userDto);

        var usersService = new UsersService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await usersService.UpdateUser(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(userId));
        _logger.ReceivedLog(LogLevel.Information, $"Updated user with ID '{user.Id}'");

        var userInDb = _yglDbContextBuilder.Get().Users.FirstOrDefault(u => u.Id == parameters.UserInformation.UserId);
        Assert.That(userInDb, Is.Not.Null);
        Assert.That(userInDb!.Country, Is.EqualTo(parameters.Country));
        Assert.That(userInDb.Description, Is.EqualTo(parameters.Description));
        Assert.That(userInDb.DateOfBirth, Is.EqualTo(parameters.DateOfBirth));
    }

    [Test]
    public async Task UpdateUser_UserDoesNotExists_ReturnsUserNotFoundError()
    {
        //ARRANGE
        var parameters = _fixture.Create<UserUpdateParameters>();
        var usersService = new UsersService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await usersService.UpdateUser(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(UsersError.UserNotFound));
        _logger.ReceivedLog(LogLevel.Warning, $"User with ID '{parameters.UserInformation.UserId}' not found.");
    }

    #endregion
}
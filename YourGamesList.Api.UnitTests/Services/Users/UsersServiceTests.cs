using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Model;
using YourGamesList.Api.Services;
using YourGamesList.Api.Services.ModelMappers;
using YourGamesList.Api.Services.Users;
using YourGamesList.Api.Services.Users.Model;
using YourGamesList.Contracts.Dto;
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
    private TimeProvider _timeProvider;
    private ICountriesService _countriesService;

    private TestYglDbContextBuilder _yglDbContextBuilder;
    private IDbContextFactory<YglDbContext> _dbContextFactory;

    private YglDbContext _yglDbContext;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<UsersService>>();
        _yglDatabaseAndDtoMapper = Substitute.For<IYglDatabaseAndDtoMapper>();
        _timeProvider = Substitute.For<TimeProvider>();
        _countriesService = Substitute.For<ICountriesService>();

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

        var usersService = new UsersService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _countriesService, _timeProvider);

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
        var usersService = new UsersService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _countriesService, _timeProvider);

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
            .With(x => x.Description, "ebebe")
            .With(x => x.DateOfBirth, (DateTimeOffset?) null)
            .WithAutoProperties()
            .Create();
        var time = _fixture.Create<DateTimeOffset>();
        _timeProvider.GetUtcNow().Returns(time);
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
        _countriesService.ValidateThreeLetterIsoRegionName(parameters.Country).Returns(true);

        var usersService = new UsersService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _countriesService, _timeProvider);

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
        Assert.That(userInDb.LastModifiedDate, Is.EqualTo(time));
    }

    [Test]
    public async Task UpdateUser_OnValidationError_WrongCountry_ReturnsUserUpdateWrongInputDataError()
    {
        //ARRANGE
        var userId = Guid.NewGuid();
        var parameters = _fixture.Build<UserUpdateParameters>()
            .With(x => x.UserInformation, _fixture.Build<JwtUserInformation>()
                .With(x => x.UserId, userId)
                .WithAutoProperties()
                .Create())
            .With(x => x.Description, "ebebe")
            .With(x => x.DateOfBirth, (DateTimeOffset?) null)
            .WithAutoProperties()
            .Create();
        var time = _fixture.Create<DateTimeOffset>();
        _timeProvider.GetUtcNow().Returns(time);
        _countriesService.ValidateThreeLetterIsoRegionName(parameters.Country).Returns(false);

        var usersService = new UsersService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _countriesService, _timeProvider);

        //ACT
        var result = await usersService.UpdateUser(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(UsersError.UserUpdateWrongInputData));
        _logger.ReceivedLog(LogLevel.Warning, $"User with ID '{parameters.UserInformation.UserId}' cannot be updated due to validation errors.");
        _logger.ReceivedLog(LogLevel.Information, $"Validation of '{nameof(UserUpdateParameters)}' failed. Reason: wrong country name.");
    }

    [Test]
    public async Task UpdateUser_OnValidationError_WrongDescription_ReturnsUserUpdateWrongInputDataError()
    {
        //ARRANGE
        var userId = Guid.NewGuid();
        var parameters = _fixture.Build<UserUpdateParameters>()
            .With(x => x.UserInformation, _fixture.Build<JwtUserInformation>()
                .With(x => x.UserId, userId)
                .WithAutoProperties()
                .Create())
            .With(x => x.Description, new string(_fixture.CreateMany<char>(1000).ToArray()))
            .With(x => x.DateOfBirth, (DateTimeOffset?) null)
            .WithAutoProperties()
            .Create();
        var time = _fixture.Create<DateTimeOffset>();
        _timeProvider.GetUtcNow().Returns(time);
        _countriesService.ValidateThreeLetterIsoRegionName(parameters.Country).Returns(true);

        var usersService = new UsersService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _countriesService, _timeProvider);

        //ACT
        var result = await usersService.UpdateUser(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(UsersError.UserUpdateWrongInputData));
        _logger.ReceivedLog(LogLevel.Warning, $"User with ID '{parameters.UserInformation.UserId}' cannot be updated due to validation errors.");
        _logger.ReceivedLog(LogLevel.Information, $"Validation of '{nameof(UserUpdateParameters)}' failed. Reason: wrong description.");
    }

    [Test]
    public async Task UpdateUser_OnValidationError_WrongDateOfBirth_ReturnsUserUpdateWrongInputDataError()
    {
        //ARRANGE
        var userId = Guid.NewGuid();
        var parameters = _fixture.Build<UserUpdateParameters>()
            .With(x => x.UserInformation, _fixture.Build<JwtUserInformation>()
                .With(x => x.UserId, userId)
                .WithAutoProperties()
                .Create())
            .With(x => x.Description, "ebebe")
            .With(x => x.DateOfBirth, DateTime.UtcNow)
            .WithAutoProperties()
            .Create();
        var time = _fixture.Create<DateTimeOffset>();
        _timeProvider.GetUtcNow().Returns(time);
        _countriesService.ValidateThreeLetterIsoRegionName(parameters.Country).Returns(true);

        var usersService = new UsersService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _countriesService, _timeProvider);

        //ACT
        var result = await usersService.UpdateUser(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(UsersError.UserUpdateWrongInputData));
        _logger.ReceivedLog(LogLevel.Warning, $"User with ID '{parameters.UserInformation.UserId}' cannot be updated due to validation errors.");
        _logger.ReceivedLog(LogLevel.Information, $"Validation of '{nameof(UserUpdateParameters)}' failed. Reason: wrong date of birth.");
    }

    [Test]
    public async Task UpdateUser_UserDoesNotExists_ReturnsUserNotFoundError()
    {
        //ARRANGE
        var userId = Guid.NewGuid();
        var parameters = _fixture.Build<UserUpdateParameters>()
            .With(x => x.UserInformation, _fixture.Build<JwtUserInformation>()
                .With(x => x.UserId, userId)
                .WithAutoProperties()
                .Create())
            .With(x => x.Description, "ebebe")
            .With(x => x.DateOfBirth, (DateTimeOffset?) null)
            .WithAutoProperties()
            .Create();
        var time = _fixture.Create<DateTimeOffset>();
        _timeProvider.GetUtcNow().Returns(time);
        _countriesService.ValidateThreeLetterIsoRegionName(parameters.Country).Returns(true);
        var usersService = new UsersService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _countriesService, _timeProvider);

        //ACT
        var result = await usersService.UpdateUser(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(UsersError.UserNotFound));
        _logger.ReceivedLog(LogLevel.Warning, $"User with ID '{parameters.UserInformation.UserId}' not found.");
    }

    #endregion
}
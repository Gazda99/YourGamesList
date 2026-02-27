using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Model;
using YourGamesList.Api.Services.ModelMappers;
using YourGamesList.Api.Services.Ygl.Lists;
using YourGamesList.Api.Services.Ygl.Lists.Model;
using YourGamesList.Contracts.Dto;
using YourGamesList.Database;
using YourGamesList.Database.Entities;
using YourGamesList.Database.TestUtils;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.Ygl.Lists;

public class ListsServiceTests
{
    private IFixture _fixture;
    private ILogger<ListsService> _logger;
    private IYglDatabaseAndDtoMapper _yglDatabaseAndDtoMapper;
    private TimeProvider _timeProvider;

    private TestYglDbContextBuilder _yglDbContextBuilder;
    private IDbContextFactory<YglDbContext> _dbContextFactory;

    private YglDbContext _yglDbContext;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<ListsService>>();
        _yglDatabaseAndDtoMapper = Substitute.For<IYglDatabaseAndDtoMapper>();

        _yglDbContextBuilder = TestYglDbContextBuilder.Build();
        _dbContextFactory = Substitute.For<IDbContextFactory<YglDbContext>>();
        _dbContextFactory.CreateDbContext().Returns(_yglDbContextBuilder.Get());
        _timeProvider = Substitute.For<TimeProvider>();
    }

    #region CreateList

    [Test]
    public async Task CreateList_SuccessfulScenario()
    {
        //ARRANGE
        var userInformation = _fixture.Create<JwtUserInformation>();
        var listName = _fixture.Create<string>();
        var desc = _fixture.Create<string>();
        var time = _fixture.Create<DateTimeOffset>();
        var isPublic = _fixture.Create<bool>();
        _timeProvider.GetUtcNow().Returns(time);
        var parameters = _fixture.Build<CreateListParameters>()
            .With(x => x.Description, desc)
            .With(x => x.IsPublic, isPublic)
            .With(x => x.UserInformation, userInformation)
            .With(x => x.ListName, listName)
            .Create();
        var users = new List<User>()
        {
            TestYglDbContextBuilder.CreateUser(id: userInformation.UserId, username: userInformation.Username)
        };
        _yglDbContextBuilder.WithUserDbSet(users);

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.CreateList(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        _logger.ReceivedLog(LogLevel.Information, ["Created new list with id ", $"for user '{userInformation.UserId}'."]);
        var createdList = _yglDbContextBuilder.Get().Lists.FirstOrDefault(u => u.UserId == userInformation.UserId && u.Name.ToLower() == listName.ToLower());
        Assert.That(createdList, Is.Not.Null);
        Assert.That(createdList.Name, Is.EqualTo(listName));
        Assert.That(createdList.Description, Is.EqualTo(desc));
        Assert.That(createdList.CreatedDate, Is.EqualTo(time));
        Assert.That(createdList.IsPublic, Is.EqualTo(isPublic));
    }

    [Test]
    public async Task CreateList_UserHasAlreadyListWithSameName_ReturnsListAlreadyExistsError()
    {
        //ARRANGE
        var userInformation = _fixture.Create<JwtUserInformation>();
        var listName = _fixture.Create<string>();
        var parameters = _fixture.Build<CreateListParameters>()
            .With(x => x.UserInformation, userInformation)
            .With(x => x.ListName, listName)
            .Create();
        var users = new List<User>()
        {
            TestYglDbContextBuilder.CreateUser(id: userInformation.UserId, username: userInformation.Username)
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = Guid.NewGuid(),
                Name = listName,
                UserId = userInformation.UserId,
                CreatedDate = _fixture.Create<DateTimeOffset>()
            }
        };
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);


        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.CreateList(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListAlreadyExists));
        _logger.ReceivedLog(LogLevel.Information, $"'{listName}' already exists.");
    }

    #endregion

    #region GetList

    [Test]
    public async Task GetList_SuccessfulScenario()
    {
        //ARRANGE
        var userInformation = _fixture.Create<JwtUserInformation>();
        var listName = _fixture.Create<string>();
        var listId = Guid.NewGuid();
        var dto1 = _fixture.Create<GamesListDto>();

        var users = new List<User>()
        {
            TestYglDbContextBuilder.CreateUser(id: userInformation.UserId, username: userInformation.Username)
        };
        var gl1 = new GamesList()
        {
            Id = listId,
            Name = listName,
            UserId = userInformation.UserId,
            IsPublic = true,
            CanBeDeleted = true,
            CreatedDate = _fixture.Create<DateTimeOffset>()
        };
        var gamesLists = new List<GamesList>() { gl1 };
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);

        _yglDatabaseAndDtoMapper.Map(Arg.Is<GamesList>(g => g.Id == gl1.Id)).Returns(dto1);

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.GetList(userInformation, listId, false);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(dto1));
        _logger.ReceivedLog(LogLevel.Information, $"Found list with id {listId.ToString()}.");
    }

    [Test]
    public async Task GetList_ListDoesNotExists_ReturnsListNotFoundError()
    {
        //ARRANGE
        var userInformation = _fixture.Create<JwtUserInformation>();
        var listId = Guid.NewGuid();

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.GetList(userInformation, listId, false);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListNotFound));
        _logger.ReceivedLog(LogLevel.Information, $"No lists found with id '{listId.ToString()}'.");
    }

    #endregion

    #region SearchLists

    [Test]
    public async Task SearchLists_SuccessfulScenario_UsingListName()
    {
        //ARRANGE
        var listNameCommon = "HeLLo";
        var listName1 = "Hello World";
        var listName2 = "World hello!";
        var listId = Guid.NewGuid();
        var listId2 = Guid.NewGuid();
        var dto1 = _fixture.Create<GamesListDto>();
        var dto2 = _fixture.Create<GamesListDto>();
        var userId = Guid.NewGuid();
        var parameters = _fixture.Build<SearchListsParameters>()
            .With(x => x.ListName, listNameCommon)
            .With(x => x.UserName, string.Empty)
            .With(x => x.Skip, 0)
            .With(x => x.Take, 10)
            .Create();
        var users = new List<User>()
        {
            TestYglDbContextBuilder.CreateUser(id: userId)
        };
        var gl1 = new GamesList()
        {
            Id = listId,
            Name = listName1,
            UserId = userId,
            CanBeDeleted = false,
            //This will be returned because it is public
            IsPublic = true,
            CreatedDate = _fixture.Create<DateTimeOffset>()
        };
        var gl2 = new GamesList()
        {
            Id = listId2,
            Name = listName2,
            UserId = userId,
            CanBeDeleted = true,
            //This won't be returned because it is not public
            IsPublic = false,
            CreatedDate = _fixture.Create<DateTimeOffset>()
        };
        var gamesLists = new List<GamesList>() { gl1, gl2 };
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);

        _yglDatabaseAndDtoMapper.Map(Arg.Is<GamesList>(g => g.Id == gl1.Id)).Returns(dto1);
        _yglDatabaseAndDtoMapper.Map(Arg.Is<GamesList>(g => g.Id == gl2.Id)).Returns(dto2);

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.SearchLists(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Count, Is.EqualTo(1));
        Assert.That(result.Value.Contains(dto1));
        Assert.That(!result.Value.Contains(dto2));
        _logger.ReceivedLog(LogLevel.Information, $"Found '{1.ToString()}' lists.");
    }

    [Test]
    public async Task SearchLists_SuccessfulScenario_UsingUsername()
    {
        //ARRANGE
        var username = "gazda";
        var listId = Guid.NewGuid();
        var listId2 = Guid.NewGuid();
        var dto1 = _fixture.Create<GamesListDto>();
        var dto2 = _fixture.Create<GamesListDto>();
        var userId = Guid.NewGuid();
        var parameters = _fixture.Build<SearchListsParameters>()
            .With(x => x.ListName, string.Empty)
            .With(x => x.UserName, username)
            .With(x => x.Skip, 0)
            .With(x => x.Take, 10)
            .Create();
        var users = new List<User>()
        {
            TestYglDbContextBuilder.CreateUser(id: userId, username: username)
        };
        var gl1 = new GamesList()
        {
            Id = listId,
            Name = _fixture.Create<string>(),
            UserId = userId,
            CanBeDeleted = false,
            IsPublic = true,
            CreatedDate = _fixture.Create<DateTimeOffset>()
        };
        var gl2 = new GamesList()
        {
            Id = listId2,
            Name = _fixture.Create<string>(),
            UserId = userId,
            CanBeDeleted = true,
            IsPublic = true,
            CreatedDate = _fixture.Create<DateTimeOffset>()
        };
        var gamesLists = new List<GamesList>() { gl1, gl2 };
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);

        _yglDatabaseAndDtoMapper.Map(Arg.Is<GamesList>(g => g.Id == gl1.Id)).Returns(dto1);
        _yglDatabaseAndDtoMapper.Map(Arg.Is<GamesList>(g => g.Id == gl2.Id)).Returns(dto2);

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.SearchLists(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Count, Is.EqualTo(2));
        Assert.That(result.Value.Contains(dto1));
        Assert.That(result.Value.Contains(dto2));
        _logger.ReceivedLog(LogLevel.Information, $"Found '{gamesLists.Count}' lists.");
    }

    [Test]
    public async Task SearchLists_NoSearchParameters_ReturnsBadRequestError()
    {
        //ARRANGE
        var parameters = _fixture
            .Build<SearchListsParameters>()
            .With(x => x.ListName, string.Empty)
            .With(x => x.UserName, string.Empty)
            .Create();

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.SearchLists(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(ListsError.BadRequest));
        _logger.ReceivedLog(LogLevel.Information, "No search parameters provided.");
    }

    [Test]
    public async Task SearchLists_WhenNoListsFound_ReturnsListNotFoundError()
    {
        //ARRANGE
        var parameters = _fixture.Create<SearchListsParameters>();

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.SearchLists(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListNotFound));
        _logger.ReceivedLog(LogLevel.Information, "No lists found for the provided search parameters.");
    }

    #endregion

    #region GetSelfLists

    [Test]
    public async Task GetSelfLists_SuccessfulScenario()
    {
        //ARRANGE
        var userInformation = _fixture.Create<JwtUserInformation>();
        var listName = _fixture.Create<string>();
        var listName2 = _fixture.Create<string>();
        var listId = Guid.NewGuid();
        var listId2 = Guid.NewGuid();
        var dto1 = _fixture.Create<GamesListDto>();
        var dto2 = _fixture.Create<GamesListDto>();

        var users = new List<User>()
        {
            TestYglDbContextBuilder.CreateUser(id: userInformation.UserId, username: userInformation.Username)
        };
        var gl1 = new GamesList()
        {
            Id = listId,
            Name = listName,
            UserId = userInformation.UserId,
            CanBeDeleted = false,
            CreatedDate = _fixture.Create<DateTimeOffset>()
        };
        var gl2 = new GamesList()
        {
            Id = listId2,
            Name = listName2,
            UserId = userInformation.UserId,
            CanBeDeleted = true,
            CreatedDate = _fixture.Create<DateTimeOffset>()
        };
        var gamesLists = new List<GamesList>() { gl1, gl2 };

        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);

        _yglDatabaseAndDtoMapper.Map(Arg.Is<GamesList>(g => g.Id == gl1.Id)).Returns(dto1);
        _yglDatabaseAndDtoMapper.Map(Arg.Is<GamesList>(g => g.Id == gl2.Id)).Returns(dto2);

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.GetSelfLists(userInformation, true);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Count, Is.EqualTo(2));
        Assert.That(result.Value.Contains(dto1));
        Assert.That(result.Value.Contains(dto2));
        _logger.ReceivedLog(LogLevel.Information, $"Found '{gamesLists.Count}' lists for the user '{userInformation.UserId}'.");
    }

    [Test]
    public async Task GetSelfLists_ListDoesNotExists_ReturnsListNotFoundError()
    {
        //ARRANGE
        var userInformation = _fixture.Create<JwtUserInformation>();
        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.GetSelfLists(userInformation, false);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListNotFound));
        _logger.ReceivedLog(LogLevel.Information, $"No lists found for the user '{userInformation.UserId}'.");
    }

    #endregion

    #region UpdateList

    [Test]
    public async Task UpdateList_SuccessfulScenario()
    {
        //ARRANGE
        var listId = Guid.NewGuid();
        var parameters = _fixture.Build<UpdateListParameters>()
            .With(x => x.ListId, listId)
            .Create();
        var time = _fixture.Create<DateTimeOffset>();
        _timeProvider.GetUtcNow().Returns(time);
        var users = new List<User>()
        {
            TestYglDbContextBuilder.CreateUser(id: parameters.UserInformation.UserId, username: parameters.UserInformation.Username)
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = listId,
                Name = _fixture.Create<string>(),
                UserId = parameters.UserInformation.UserId,
                CreatedDate = _fixture.Create<DateTimeOffset>()
            }
        };
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.UpdateList(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(listId));
        _logger.ReceivedLog(LogLevel.Information, "List updated successfully.");
        var updatedList = _yglDbContextBuilder.Get().Lists.FirstOrDefault(x => x.Id == listId);
        Assert.That(updatedList, Is.Not.Null);
        Assert.That(updatedList.LastModifiedDate, Is.EqualTo(time));
    }

    [Test]
    public async Task UpdateList_WhenNoListsFound_ReturnsListNotFoundError()
    {
        //ARRANGE
        var parameters = _fixture.Create<UpdateListParameters>();

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.UpdateList(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListNotFound));
        _logger.ReceivedLog(LogLevel.Information, $"No lists found for id '{parameters.ListId}' and user '{parameters.UserInformation.UserId}'.");
    }

    [Test]
    public async Task UpdateList_UserHasAlreadyListWithSameNameAsNewlyRequestedName_ReturnsListAlreadyExistsError()
    {
        //ARRANGE
        var listName = _fixture.Create<string>();
        var listName2 = _fixture.Create<string>();
        var listId = Guid.NewGuid();
        var parameters = _fixture.Build<UpdateListParameters>()
            .With(x => x.ListId, listId)
            .With(x => x.Name, listName2)
            .Create();
        var users = new List<User>()
        {
            TestYglDbContextBuilder.CreateUser(id: parameters.UserInformation.UserId, username: parameters.UserInformation.Username)
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = listId,
                Name = listName,
                UserId = parameters.UserInformation.UserId,
                CreatedDate = _fixture.Create<DateTimeOffset>()
            },
            new GamesList()
            {
                Id = Guid.NewGuid(),
                Name = listName2,
                UserId = parameters.UserInformation.UserId,
                CreatedDate = _fixture.Create<DateTimeOffset>()
            }
        };
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);


        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.UpdateList(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListAlreadyExists));
        _logger.ReceivedLog(LogLevel.Information,
            $"Cannot rename list to '{parameters.Name}' because it already exists for user '{parameters.UserInformation.UserId}'.");
    }

    #endregion

    #region DeleteList

    [Test]
    public async Task DeleteList_SuccessfulScenario()
    {
        //ARRANGE
        var userInformation = _fixture.Create<JwtUserInformation>();
        var listName = _fixture.Create<string>();
        var listId = Guid.NewGuid();

        var users = new List<User>()
        {
            TestYglDbContextBuilder.CreateUser(id: userInformation.UserId, username: userInformation.Username)
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = listId,
                Name = listName,
                UserId = userInformation.UserId,
                CanBeDeleted = true,
                CreatedDate = _fixture.Create<DateTimeOffset>()
            }
        };
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.DeleteList(userInformation, listId);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        _logger.ReceivedLog(LogLevel.Information, $"Deleting list '{listId}'");
        var deletedList = _yglDbContextBuilder.Get().Lists.FirstOrDefault(u => u.Id == listId);
        Assert.That(deletedList, Is.Null);
    }

    [Test]
    public async Task DeleteList_ListDoesNotExists_ReturnsListNotFoundError()
    {
        //ARRANGE
        var userInformation = _fixture.Create<JwtUserInformation>();
        var listId = Guid.NewGuid();

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.DeleteList(userInformation, listId);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListNotFound));
        _logger.ReceivedLog(LogLevel.Information, $"List with id '{listId}' not found.");
    }

    [Test]
    public async Task DeleteList_ListHardLocked_ReturnsListHardLockedError()
    {
        //ARRANGE
        var userInformation = _fixture.Create<JwtUserInformation>();
        var listName = _fixture.Create<string>();
        var listId = Guid.NewGuid();

        var users = new List<User>()
        {
            TestYglDbContextBuilder.CreateUser(id: userInformation.UserId, username: userInformation.Username)
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = listId,
                Name = listName,
                UserId = userInformation.UserId,
                CanBeDeleted = false,
                CreatedDate = _fixture.Create<DateTimeOffset>()
            }
        };
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.DeleteList(userInformation, listId);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListHardLocked));
        _logger.ReceivedLog(LogLevel.Information, $"List with id '{listId}' cannot be deleted due to hard lock.");
    }

    #endregion


    #region AddListEntries

    [Test]
    public async Task AddListEntries_SuccessfulScenario()
    {
        //ARRANGE
        var gameId = _fixture.Create<long>();
        var entryToAdd = _fixture.Build<EntryToAddParameter>()
            .With(x => x.GameId, gameId)
            .Create();
        var parameters = _fixture.Build<AddEntriesToListParameter>()
            .With(x => x.EntriesToAdd, [entryToAdd])
            .Create();
        var time = _fixture.Create<DateTimeOffset>();
        _timeProvider.GetUtcNow().Returns(time);

        var games = new List<Game>()
        {
            new Game()
            {
                Id = gameId,
                Name = _fixture.Create<string>()
            }
        };
        var users = new List<User>()
        {
            TestYglDbContextBuilder.CreateUser(id: parameters.UserInformation.UserId, username: parameters.UserInformation.Username)
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = parameters.ListId,
                Name = _fixture.Create<string>(),
                UserId = parameters.UserInformation.UserId,
                CreatedDate = _fixture.Create<DateTimeOffset>()
            }
        };

        _yglDbContextBuilder.WithGamesDbSet(games);
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.AddListEntries(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Count, Is.EqualTo(1));
        _logger.ReceivedLog(LogLevel.Information, [$"Successfully added '{parameters.EntriesToAdd.Length}' entries", $"to list '{parameters.ListId}'."]);
        var list = _yglDbContextBuilder.Get().Lists.Include(x => x.Entries).FirstOrDefault(u => u.Id == parameters.ListId);
        Assert.That(list, Is.Not.Null);
        Assert.That(list.Entries.Count, Is.EqualTo(1));
        Assert.That(list.Entries.First().GameId, Is.EqualTo(gameId));
        Assert.That(list.LastModifiedDate, Is.EqualTo(time));
        foreach (var entry in list.Entries)
        {
            Assert.That(entry.CreatedDate, Is.EqualTo(time));
        }
    }

    [Test]
    public async Task AddListEntries_ListDoesNotExists_ReturnsListNotFoundError()
    {
        //ARRANGE
        var parameters = _fixture.Create<AddEntriesToListParameter>();

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.AddListEntries(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListNotFound));
        _logger.ReceivedLog(LogLevel.Information, $"No lists found for id '{parameters.ListId}' and user '{parameters.UserInformation.UserId}'.");
    }

    [Test]
    public async Task AddListEntries_NoNewEntries_ReturnsEmptySuccess()
    {
        //ARRANGE
        var gameId = _fixture.Create<long>();
        var entryToAdd = _fixture.Build<EntryToAddParameter>()
            .With(x => x.GameId, gameId)
            .Create();
        var parameters = _fixture.Build<AddEntriesToListParameter>()
            .With(x => x.EntriesToAdd, [entryToAdd])
            .Create();

        var games = new List<Game>()
        {
            new Game()
            {
                Id = gameId,
                Name = _fixture.Create<string>()
            }
        };
        var users = new List<User>()
        {
            TestYglDbContextBuilder.CreateUser(id: parameters.UserInformation.UserId, username: parameters.UserInformation.Username)
        };
        var gameListEntries = new List<GameListEntry>()
        {
            new GameListEntry()
            {
                Id = Guid.NewGuid(),
                GamesListId = parameters.ListId,
                GameId = gameId,
                CreatedDate = _fixture.Create<DateTimeOffset>()
            }
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = parameters.ListId,
                Name = _fixture.Create<string>(),
                UserId = parameters.UserInformation.UserId,
                CreatedDate = _fixture.Create<DateTimeOffset>()
            }
        };

        _yglDbContextBuilder.WithGamesDbSet(games);
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);
        _yglDbContextBuilder.WithGameListEntriesDbSet(gameListEntries);

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.AddListEntries(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Empty);
        _logger.ReceivedLog(LogLevel.Information, "No new entries to add to the list.");
    }

    #endregion

    #region DeleteListEntries

    [Test]
    public async Task DeleteListEntries_SuccessfulScenario()
    {
        //ARRANGE
        var gameId = _fixture.Create<long>();
        var listEntryId1 = Guid.NewGuid();
        var listEntryId2 = Guid.NewGuid();
        var listId = Guid.NewGuid();
        var parameters = _fixture.Build<DeleteListEntriesParameter>()
            .With(x => x.ListId, listId)
            .With(x => x.EntriesToRemove, [listEntryId1])
            .Create();

        var games = new List<Game>()
        {
            new Game()
            {
                Id = gameId,
                Name = _fixture.Create<string>()
            }
        };
        var users = new List<User>()
        {
            TestYglDbContextBuilder.CreateUser(id: parameters.UserInformation.UserId, username: parameters.UserInformation.Username)
        };
        var gameListEntries = new List<GameListEntry>()
        {
            new GameListEntry()
            {
                Id = listEntryId1,
                GamesListId = listId,
                GameId = gameId,
                CreatedDate = _fixture.Create<DateTimeOffset>()
            },
            new GameListEntry()
            {
                Id = listEntryId2,
                GamesListId = listId,
                GameId = gameId,
                CreatedDate = _fixture.Create<DateTimeOffset>()
            }
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = listId,
                Name = _fixture.Create<string>(),
                UserId = parameters.UserInformation.UserId,
                CreatedDate = _fixture.Create<DateTimeOffset>()
            }
        };
        _yglDbContextBuilder.WithGamesDbSet(games);
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);
        _yglDbContextBuilder.WithGameListEntriesDbSet(gameListEntries);

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.DeleteListEntries(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Count, Is.EqualTo(1));
        Assert.That(result.Value, Contains.Item(listEntryId1));
        _logger.ReceivedLog(LogLevel.Information,
            [$"Deleted '{parameters.EntriesToRemove.Length}' entries", listEntryId1.ToString(), $"from list '{parameters.ListId}'."]);
        var list = _yglDbContextBuilder.Get().Lists.Include(x => x.Entries).FirstOrDefault(u => u.Id == listId);
        Assert.That(list, Is.Not.Null);
        //Only non deleted item should remain in the list
        Assert.That(list.Entries.Count, Is.EqualTo(1));
        Assert.That(list.Entries.First().Id, Is.EqualTo(listEntryId2));
    }

    [Test]
    public async Task DeleteListEntries_ListDoesNotExists_ReturnsListNotFoundError()
    {
        //ARRANGE
        var parameters = _fixture.Create<DeleteListEntriesParameter>();

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.DeleteListEntries(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListNotFound));
        _logger.ReceivedLog(LogLevel.Information, $"No lists found for id '{parameters.ListId}' and user '{parameters.UserInformation.UserId}'.");
    }

    [Test]
    public async Task DeleteListEntries_EntriesDontExist_ReturnsEmptySuccess()
    {
        //ARRANGE
        var listId = Guid.NewGuid();
        var parameters = _fixture.Build<DeleteListEntriesParameter>()
            .With(x => x.ListId, listId)
            .Create();

        var users = new List<User>()
        {
            TestYglDbContextBuilder.CreateUser(id: parameters.UserInformation.UserId, username: parameters.UserInformation.Username)
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = listId,
                Name = _fixture.Create<string>(),
                UserId = parameters.UserInformation.UserId,
                CreatedDate = _fixture.Create<DateTimeOffset>()
            }
        };
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.DeleteListEntries(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Empty);
        _logger.ReceivedLog(LogLevel.Information, $"No entries found to delete from the list {parameters.ListId}'.");
    }

    #endregion

    #region UpdateListEntries

    [Test]
    public async Task UpdateListEntries_SuccessfulScenario()
    {
        //ARRANGE
        var gameId = _fixture.Create<long>();
        var listEntryId1 = Guid.NewGuid();
        var listEntryId2 = Guid.NewGuid();
        var listId = Guid.NewGuid();
        var completionStatusDto = _fixture.Create<CompletionStatusDto>();
        var completionStatus = _fixture.Create<CompletionStatus>();
        var gameDistributionDto = _fixture.Create<GameDistributionDto>();
        var gameDistribution = _fixture.Create<GameDistribution>();
        var platformDto = _fixture.Create<PlatformDto>();
        var platform = _fixture.Create<Platform>();
        var time = _fixture.Create<DateTimeOffset>();
        _timeProvider.GetUtcNow().Returns(time);
        //This should be updated
        var entryToUpdateParameter = _fixture.Build<EntryToUpdateParameter>()
            .With(x => x.EntryId, listEntryId1)
            .With(x => x.CompletionStatus, completionStatusDto)
            .Create();
        var entriesToUpdate = new[]
        {
            entryToUpdateParameter,
            //This should not be updated, because it does not exist in the list
            _fixture.Build<EntryToUpdateParameter>()
                .With(x => x.EntryId, listEntryId2)
                .Create()
        };
        var parameters = _fixture.Build<UpdateListEntriesParameter>()
            .With(x => x.ListId, listId)
            .With(x => x.EntriesToUpdate, entriesToUpdate)
            .Create();

        var games = new List<Game>()
        {
            new Game()
            {
                Id = gameId,
                Name = _fixture.Create<string>()
            }
        };
        var users = new List<User>()
        {
            TestYglDbContextBuilder.CreateUser(id: parameters.UserInformation.UserId, username: parameters.UserInformation.Username)
        };
        var gameListEntries = new List<GameListEntry>()
        {
            new GameListEntry()
            {
                Id = listEntryId1,
                GamesListId = listId,
                GameId = gameId,
                CreatedDate = _fixture.Create<DateTimeOffset>()
            }
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = listId,
                Name = _fixture.Create<string>(),
                UserId = parameters.UserInformation.UserId,
                CreatedDate = _fixture.Create<DateTimeOffset>()
            }
        };
        _yglDbContextBuilder.WithGamesDbSet(games);
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);
        _yglDbContextBuilder.WithGameListEntriesDbSet(gameListEntries);


        _yglDatabaseAndDtoMapper.Map(completionStatusDto).Returns(completionStatus);
        _yglDatabaseAndDtoMapper.Map(gameDistributionDto).Returns(gameDistribution);
        _yglDatabaseAndDtoMapper.Map(platformDto).Returns(platform);

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.UpdateListEntries(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        _logger.ReceivedLog(LogLevel.Information, $"Entry to update '{listEntryId2}' not found in the list '{listId}'.");
        _logger.ReceivedLog(LogLevel.Information, ["Updated '1' entries", $"from list '{listId}'."]);
        var list = _yglDbContextBuilder.Get().Lists.Include(x => x.Entries).FirstOrDefault(u => u.Id == parameters.ListId);
        Assert.That(list.LastModifiedDate, Is.EqualTo(time));
        var updatedEntry = list.Entries.FirstOrDefault(e => e.Id == listEntryId1);
        Assert.That(updatedEntry, Is.Not.Null);
        Assert.That(updatedEntry.IsStarred, Is.EqualTo(entryToUpdateParameter.IsStarred));
        Assert.That(updatedEntry.Rating, Is.EqualTo(entryToUpdateParameter.Rating));
        Assert.That(updatedEntry.Description, Is.EqualTo(entryToUpdateParameter.Description));
        Assert.That(updatedEntry.CompletionStatus, Is.EqualTo(completionStatus));
        Assert.That(updatedEntry.LastModifiedDate, Is.EqualTo(time));
    }

    [Test]
    public async Task UpdateListEntries_ListDoesNotExists_ReturnsListNotFoundError()
    {
        //ARRANGE
        var parameters = _fixture.Create<UpdateListEntriesParameter>();

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.UpdateListEntries(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListNotFound));
        _logger.ReceivedLog(LogLevel.Information, $"No lists found for id '{parameters.ListId}' and user '{parameters.UserInformation.UserId}'.");
    }

    #endregion

    #region AddOwnershipInfo

    [Test]
    public async Task AddOwnershipInfo_SuccessfulScenario()
    {
        //ARRANGE
        var gameId = _fixture.Create<long>();
        var gameListId = _fixture.Create<Guid>();
        var gameListEntryId = _fixture.Create<Guid>();
        var platformDto = _fixture.Create<PlatformDto>();
        var platform = _fixture.Create<Platform>();
        var emulatorDto = _fixture.Create<EmulatorDto>();
        var emulator = _fixture.Create<Emulator>();
        var gameDistributionDto = _fixture.Create<GameDistributionDto>();
        var gameDistribution = _fixture.Create<GameDistribution>();
        var entryToAdd = _fixture.Build<OwnershipsToAddParameter>()
            .With(x => x.Platform, platformDto)
            .With(x => x.GameDistribution, gameDistributionDto)
            .With(x => x.EmulatedOn, emulatorDto)
            .Create();
        var parameters = _fixture.Build<AddOwnershipInfoToEntryParameters>()
            .With(x => x.ListEntryId, gameListEntryId)
            .With(x => x.OwnershipsToAdd, [entryToAdd])
            .Create();
        var time = _fixture.Create<DateTimeOffset>();
        _timeProvider.GetUtcNow().Returns(time);

        var games = new List<Game>()
        {
            new Game()
            {
                Id = gameId,
                Name = _fixture.Create<string>()
            }
        };
        var users = new List<User>()
        {
            TestYglDbContextBuilder.CreateUser(id: parameters.UserInformation.UserId, username: parameters.UserInformation.Username)
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = gameListId,
                Name = _fixture.Create<string>(),
                UserId = parameters.UserInformation.UserId,
                CreatedDate = _fixture.Create<DateTimeOffset>()
            }
        };
        var gameListEntries = new List<GameListEntry>()
        {
            new GameListEntry()
            {
                Id = gameListEntryId,
                GameId = gameId,
                GamesListId = gameListId,
                CreatedDate = _fixture.Create<DateTimeOffset>()
            }
        };

        _yglDbContextBuilder.WithGamesDbSet(games);
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);
        _yglDbContextBuilder.WithGameListEntriesDbSet(gameListEntries);

        _yglDatabaseAndDtoMapper.Map(emulatorDto).Returns(emulator);
        _yglDatabaseAndDtoMapper.Map(gameDistributionDto).Returns(gameDistribution);
        _yglDatabaseAndDtoMapper.Map(platformDto).Returns(platform);

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.AddOwnershipInfo(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Count, Is.EqualTo(1));
        _logger.ReceivedLog(LogLevel.Information, ["Successfully added '1' ownerships", $"to list entry '{gameListEntryId}'."]);
        var ownershipInfo = _yglDbContextBuilder.Get().OwnershipInfos.Include(x => x.GameListEntry).FirstOrDefault(u => u.Id == result.Value.First());
        Assert.That(ownershipInfo, Is.Not.Null);
        Assert.That(ownershipInfo.CreatedDate, Is.EqualTo((DateTimeOffset) time));
        Assert.That(ownershipInfo.IsLegit, Is.EqualTo(entryToAdd.IsLegit));
        Assert.That(ownershipInfo.EmulatedOn, Is.EqualTo(emulator));
        Assert.That(ownershipInfo.GameDistribution, Is.EqualTo(gameDistribution));
        Assert.That(ownershipInfo.Platform, Is.EqualTo(platform));
        Assert.That(ownershipInfo.WasEmulated, Is.EqualTo(entryToAdd.WasEmulated));
    }

    [Test]
    public async Task AddOwnershipInfo_ListDoesNotExists_ReturnsListNotFoundError()
    {
        //ARRANGE
        var parameters = _fixture.Create<AddOwnershipInfoToEntryParameters>();

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.AddOwnershipInfo(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListNotFound));
        _logger.ReceivedLog(LogLevel.Information, $"No list entries found for id '{parameters.ListEntryId}' and user '{parameters.UserInformation.UserId}'.");
    }

    #endregion

    #region DeleteOwnershipInfo

    [Test]
    public async Task DeleteOwnershipInfo_SuccessfulScenario()
    {
        //ARRANGE
        var gameId = _fixture.Create<long>();
        var listId = Guid.NewGuid();
        var listEntryId = Guid.NewGuid();
        var ownershipToRemoveId = Guid.NewGuid();
        var ownershipInfoStayId = Guid.NewGuid();
        var parameters = _fixture.Build<DeleteOwnershipInfoToEntryParameters>()
            .With(x => x.ListEntryId, listEntryId)
            .With(x => x.OwnershipsToRemove, [ownershipToRemoveId])
            .Create();

        var games = new List<Game>()
        {
            new Game()
            {
                Id = gameId,
                Name = _fixture.Create<string>()
            }
        };
        var users = new List<User>()
        {
            TestYglDbContextBuilder.CreateUser(id: parameters.UserInformation.UserId, username: parameters.UserInformation.Username)
        };
        var gameListEntries = new List<GameListEntry>()
        {
            new GameListEntry()
            {
                Id = listEntryId,
                GamesListId = listId,
                GameId = gameId,
                CreatedDate = _fixture.Create<DateTimeOffset>()
            }
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = listId,
                Name = _fixture.Create<string>(),
                UserId = parameters.UserInformation.UserId,
                CreatedDate = _fixture.Create<DateTimeOffset>()
            }
        };
        var ownerships = new List<OwnershipInfo>()
        {
            new OwnershipInfo()
            {
                Id = ownershipToRemoveId,
                GameListEntryId = listEntryId,
                CreatedDate = _fixture.Create<DateTimeOffset>()
            },
            new OwnershipInfo()
            {
                Id = ownershipInfoStayId,
                GameListEntryId = listEntryId,
                CreatedDate = _fixture.Create<DateTimeOffset>()
            },
        };

        _yglDbContextBuilder.WithGamesDbSet(games);
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);
        _yglDbContextBuilder.WithGameListEntriesDbSet(gameListEntries);
        _yglDbContextBuilder.WithOwnershipInfoDbSet(ownerships);

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.DeleteOwnershipInfo(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Count, Is.EqualTo(1));
        Assert.That(result.Value, Contains.Item(ownershipToRemoveId));
        _logger.ReceivedLog(LogLevel.Information, ["Deleted '1' ownerships", $"from list entry '{listEntryId}'."]);
        var gameListEntry = _yglDbContextBuilder.Get().GameListEntries.Include(x => x.OwnershipInfo).FirstOrDefault(u => u.Id == listEntryId);
        Assert.That(gameListEntry, Is.Not.Null);
        //Only non deleted item should remain in the list
        Assert.That(gameListEntry.OwnershipInfo.Count, Is.EqualTo(1));
        Assert.That(gameListEntry.OwnershipInfo.First().Id, Is.EqualTo(ownershipInfoStayId));
    }


    [Test]
    public async Task DeleteOwnershipInfo_ListDoesNotExists_ReturnsListNotFoundError()
    {
        //ARRANGE
        var parameters = _fixture.Create<DeleteOwnershipInfoToEntryParameters>();

        var listService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper, _timeProvider);

        //ACT
        var result = await listService.DeleteOwnershipInfo(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListNotFound));
        _logger.ReceivedLog(LogLevel.Information, $"No list entries found for id '{parameters.ListEntryId}' and user '{parameters.UserInformation.UserId}'.");
    }

    #endregion
}
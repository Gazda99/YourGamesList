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
    }

    #region CreateList

    [Test]
    public async Task CreateList_SuccessfulScenario()
    {
        //ARRANGE
        var userInformation = _fixture.Create<JwtUserInformation>();
        var listName = _fixture.Create<string>();
        var desc = _fixture.Create<string>();
        var users = new List<User>()
        {
            new User()
            {
                Id = userInformation.UserId,
                Username = userInformation.Username,
                PasswordHash = _fixture.Create<string>(),
                CreatedDate = DateTime.UtcNow,
                Salt = _fixture.Create<byte[]>()
            }
        };
        _yglDbContextBuilder.WithUserDbSet(users);

        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.CreateList(userInformation, listName, desc);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        _logger.ReceivedLog(LogLevel.Information, ["Created new list with id ", $"for user '{userInformation.UserId}'."]);
        var createdList = _yglDbContextBuilder.Get().Lists.FirstOrDefault(u => u.UserId == userInformation.UserId && u.Name.ToLower() == listName.ToLower());
        Assert.That(createdList, Is.Not.Null);
        Assert.That(createdList.Name, Is.EqualTo(listName));
        Assert.That(createdList.Desc, Is.EqualTo(desc));
    }

    [Test]
    public async Task CreateList_UserHasAlreadyListWithSameName_ReturnsListAlreadyExistsError()
    {
        //ARRANGE
        var userInformation = _fixture.Create<JwtUserInformation>();
        var listName = _fixture.Create<string>();
        var users = new List<User>()
        {
            new User()
            {
                Id = userInformation.UserId,
                Username = userInformation.Username,
                PasswordHash = _fixture.Create<string>(),
                CreatedDate = DateTime.UtcNow,
                Salt = _fixture.Create<byte[]>()
            }
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = Guid.NewGuid(),
                Name = listName,
                UserId = userInformation.UserId
            }
        };
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);


        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.CreateList(userInformation, listName);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListAlreadyExists));
        _logger.ReceivedLog(LogLevel.Information, $"'{listName}' already exists.");
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
            new User()
            {
                Id = userId,
                Username = _fixture.Create<string>(),
                PasswordHash = _fixture.Create<string>(),
                CreatedDate = DateTime.UtcNow,
                Salt = _fixture.Create<byte[]>()
            }
        };
        var gl1 = new GamesList()
        {
            Id = listId,
            Name = listName1,
            UserId = userId,
            CanBeDeleted = false
        };
        var gl2 = new GamesList()
        {
            Id = listId2,
            Name = listName2,
            UserId = userId,
            CanBeDeleted = true
        };
        var gamesLists = new List<GamesList>() { gl1, gl2 };
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);

        _yglDatabaseAndDtoMapper.Map(Arg.Is<GamesList>(g => g.Id == gl1.Id)).Returns(dto1);
        _yglDatabaseAndDtoMapper.Map(Arg.Is<GamesList>(g => g.Id == gl2.Id)).Returns(dto2);

        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.SearchLists(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Count, Is.EqualTo(2));
        Assert.That(result.Value.Contains(dto1));
        Assert.That(result.Value.Contains(dto2));
        _logger.ReceivedLog(LogLevel.Information, $"Found '{gamesLists.Count}' lists.");
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
            new User()
            {
                Id = userId,
                Username = username,
                PasswordHash = _fixture.Create<string>(),
                CreatedDate = DateTime.UtcNow,
                Salt = _fixture.Create<byte[]>()
            }
        };
        var gl1 = new GamesList()
        {
            Id = listId,
            Name = _fixture.Create<string>(),
            UserId = userId,
            CanBeDeleted = false
        };
        var gl2 = new GamesList()
        {
            Id = listId2,
            Name = _fixture.Create<string>(),
            UserId = userId,
            CanBeDeleted = true
        };
        var gamesLists = new List<GamesList>() { gl1, gl2 };
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);

        _yglDatabaseAndDtoMapper.Map(Arg.Is<GamesList>(g => g.Id == gl1.Id)).Returns(dto1);
        _yglDatabaseAndDtoMapper.Map(Arg.Is<GamesList>(g => g.Id == gl2.Id)).Returns(dto2);

        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.SearchLists(parameters);

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

        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.SearchLists(parameters);

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

        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.SearchLists(parameters);

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
            new User()
            {
                Id = userInformation.UserId,
                Username = userInformation.Username,
                PasswordHash = _fixture.Create<string>(),
                CreatedDate = DateTime.UtcNow,
                Salt = _fixture.Create<byte[]>()
            }
        };
        var gl1 = new GamesList()
        {
            Id = listId,
            Name = listName,
            UserId = userInformation.UserId,
            CanBeDeleted = false
        };
        var gl2 = new GamesList()
        {
            Id = listId2,
            Name = listName2,
            UserId = userInformation.UserId,
            CanBeDeleted = true
        };
        var gamesLists = new List<GamesList>() { gl1, gl2 };

        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);

        _yglDatabaseAndDtoMapper.Map(Arg.Is<GamesList>(g => g.Id == gl1.Id)).Returns(dto1);
        _yglDatabaseAndDtoMapper.Map(Arg.Is<GamesList>(g => g.Id == gl2.Id)).Returns(dto2);

        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.GetSelfLists(userInformation, true);

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
        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.GetSelfLists(userInformation, false);

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
        var users = new List<User>()
        {
            new User()
            {
                Id = parameters.UserInformation.UserId,
                Username = parameters.UserInformation.Username,
                PasswordHash = _fixture.Create<string>(),
                CreatedDate = DateTime.UtcNow,
                Salt = _fixture.Create<byte[]>()
            }
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = listId,
                Name = _fixture.Create<string>(),
                UserId = parameters.UserInformation.UserId
            }
        };
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);

        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.UpdateList(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(listId));
        _logger.ReceivedLog(LogLevel.Information, "List updated successfully.");
    }

    [Test]
    public async Task UpdateList_WhenNoListsFound_ReturnsListNotFoundError()
    {
        //ARRANGE
        var parameters = _fixture.Create<UpdateListParameters>();

        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.UpdateList(parameters);

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
            new User()
            {
                Id = parameters.UserInformation.UserId,
                Username = parameters.UserInformation.Username,
                PasswordHash = _fixture.Create<string>(),
                CreatedDate = DateTime.UtcNow,
                Salt = _fixture.Create<byte[]>()
            }
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = listId,
                Name = listName,
                UserId = parameters.UserInformation.UserId
            },
            new GamesList()
            {
                Id = Guid.NewGuid(),
                Name = listName2,
                UserId = parameters.UserInformation.UserId
            }
        };
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);


        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.UpdateList(parameters);

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
            new User()
            {
                Id = userInformation.UserId,
                Username = userInformation.Username,
                PasswordHash = _fixture.Create<string>(),
                CreatedDate = DateTime.UtcNow,
                Salt = _fixture.Create<byte[]>()
            }
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = listId,
                Name = listName,
                UserId = userInformation.UserId,
                CanBeDeleted = true
            }
        };
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);

        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.DeleteList(userInformation, listId);

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

        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.DeleteList(userInformation, listId);

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
            new User()
            {
                Id = userInformation.UserId,
                Username = userInformation.Username,
                PasswordHash = _fixture.Create<string>(),
                CreatedDate = DateTime.UtcNow,
                Salt = _fixture.Create<byte[]>()
            }
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = listId,
                Name = listName,
                UserId = userInformation.UserId,
                CanBeDeleted = false
            }
        };
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);

        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.DeleteList(userInformation, listId);

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
        var gameId = Guid.NewGuid();
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
                IgdbGameId = _fixture.Create<long>(),
                Name = _fixture.Create<string>()
            }
        };
        var users = new List<User>()
        {
            new User()
            {
                Id = parameters.UserInformation.UserId,
                Username = parameters.UserInformation.Username,
                PasswordHash = _fixture.Create<string>(),
                CreatedDate = DateTime.UtcNow,
                Salt = _fixture.Create<byte[]>()
            }
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = parameters.ListId,
                Name = _fixture.Create<string>(),
                UserId = parameters.UserInformation.UserId
            }
        };

        _yglDbContextBuilder.WithGamesDbSet(games);
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);

        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.AddListEntries(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Count, Is.EqualTo(1));
        _logger.ReceivedLog(LogLevel.Information, [$"Successfully added '{parameters.EntriesToAdd.Length}' entries", $"to list '{parameters.ListId}'."]);
        var list = _yglDbContextBuilder.Get().Lists.Include(x => x.Entries).FirstOrDefault(u => u.Id == parameters.ListId);
        Assert.That(list, Is.Not.Null);
        Assert.That(list.Entries.Count, Is.EqualTo(1));
        Assert.That(list.Entries.First().GameId, Is.EqualTo(gameId));
    }

    [Test]
    public async Task AddListEntries_ListDoesNotExists_ReturnsListNotFoundError()
    {
        //ARRANGE
        var parameters = _fixture.Create<AddEntriesToListParameter>();

        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.AddListEntries(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListNotFound));
        _logger.ReceivedLog(LogLevel.Information, $"No lists found for id '{parameters.ListId}' and user '{parameters.UserInformation.UserId}'.");
    }

    [Test]
    public async Task AddListEntries_NoNewEntries_ReturnsEmptySuccess()
    {
        //ARRANGE
        var gameId = Guid.NewGuid();
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
                IgdbGameId = _fixture.Create<long>(),
                Name = _fixture.Create<string>()
            }
        };
        var users = new List<User>()
        {
            new User()
            {
                Id = parameters.UserInformation.UserId,
                Username = parameters.UserInformation.Username,
                PasswordHash = _fixture.Create<string>(),
                CreatedDate = DateTime.UtcNow,
                Salt = _fixture.Create<byte[]>()
            }
        };
        var gameListEntries = new List<GameListEntry>()
        {
            new GameListEntry()
            {
                Id = Guid.NewGuid(),
                GamesListId = parameters.ListId,
                GameId = gameId
            }
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = parameters.ListId,
                Name = _fixture.Create<string>(),
                UserId = parameters.UserInformation.UserId
            }
        };

        _yglDbContextBuilder.WithGamesDbSet(games);
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);
        _yglDbContextBuilder.WithGameListEntriesDbSet(gameListEntries);

        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.AddListEntries(parameters);

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
        var gameId = Guid.NewGuid();
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
                IgdbGameId = _fixture.Create<long>(),
                Name = _fixture.Create<string>()
            }
        };
        var users = new List<User>()
        {
            new User()
            {
                Id = parameters.UserInformation.UserId,
                Username = parameters.UserInformation.Username,
                PasswordHash = _fixture.Create<string>(),
                CreatedDate = DateTime.UtcNow,
                Salt = _fixture.Create<byte[]>()
            }
        };
        var gameListEntries = new List<GameListEntry>()
        {
            new GameListEntry()
            {
                Id = listEntryId1,
                GamesListId = listId,
                GameId = gameId
            },
            new GameListEntry()
            {
                Id = listEntryId2,
                GamesListId = listId,
                GameId = gameId
            }
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = listId,
                Name = _fixture.Create<string>(),
                UserId = parameters.UserInformation.UserId
            }
        };
        _yglDbContextBuilder.WithGamesDbSet(games);
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);
        _yglDbContextBuilder.WithGameListEntriesDbSet(gameListEntries);

        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.DeleteListEntries(parameters);

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

        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.DeleteListEntries(parameters);

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
            new User()
            {
                Id = parameters.UserInformation.UserId,
                Username = parameters.UserInformation.Username,
                PasswordHash = _fixture.Create<string>(),
                CreatedDate = DateTime.UtcNow,
                Salt = _fixture.Create<byte[]>()
            }
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = listId,
                Name = _fixture.Create<string>(),
                UserId = parameters.UserInformation.UserId
            }
        };
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);

        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.DeleteListEntries(parameters);

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
        var gameId = Guid.NewGuid();
        var listEntryId1 = Guid.NewGuid();
        var listEntryId2 = Guid.NewGuid();
        var listId = Guid.NewGuid();
        var completionStatusDto = _fixture.Create<CompletionStatusDto>();
        var completionStatus = _fixture.Create<CompletionStatus>();
        var gameDistributionDto = _fixture.Create<GameDistributionDto>();
        var gameDistribution = _fixture.Create<GameDistribution>();
        var platformDto = _fixture.Create<PlatformDto>();
        var platform = _fixture.Create<Platform>();
        //This should be updated
        var entryToUpdateParameter = _fixture.Build<EntryToUpdateParameter>()
            .With(x => x.EntryId, listEntryId1)
            .With(x => x.CompletionStatus, completionStatusDto)
            .With(x => x.GameDistributions, [gameDistributionDto])
            .With(x => x.Platforms, [platformDto])
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
                IgdbGameId = _fixture.Create<long>(),
                Name = _fixture.Create<string>()
            }
        };
        var users = new List<User>()
        {
            new User()
            {
                Id = parameters.UserInformation.UserId,
                Username = parameters.UserInformation.Username,
                PasswordHash = _fixture.Create<string>(),
                CreatedDate = DateTime.UtcNow,
                Salt = _fixture.Create<byte[]>()
            }
        };
        var gameListEntries = new List<GameListEntry>()
        {
            new GameListEntry()
            {
                Id = listEntryId1,
                GamesListId = listId,
                GameId = gameId
            }
        };
        var gamesLists = new List<GamesList>()
        {
            new GamesList()
            {
                Id = listId,
                Name = _fixture.Create<string>(),
                UserId = parameters.UserInformation.UserId
            }
        };
        _yglDbContextBuilder.WithGamesDbSet(games);
        _yglDbContextBuilder.WithUserDbSet(users);
        _yglDbContextBuilder.WithListsDbSet(gamesLists);
        _yglDbContextBuilder.WithGameListEntriesDbSet(gameListEntries);


        _yglDatabaseAndDtoMapper.Map(completionStatusDto).Returns(completionStatus);
        _yglDatabaseAndDtoMapper.Map(gameDistributionDto).Returns(gameDistribution);
        _yglDatabaseAndDtoMapper.Map(platformDto).Returns(platform);

        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.UpdateListEntries(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        _logger.ReceivedLog(LogLevel.Information, $"Entry to update '{listEntryId2}' not found in the list '{listId}'.");
        _logger.ReceivedLog(LogLevel.Information, ["Updated '1' entries", $"from list '{listId}'."]);
        var list = _yglDbContextBuilder.Get().Lists.Include(x => x.Entries).FirstOrDefault(u => u.Id == parameters.ListId);
        var updatedEntry = list.Entries.FirstOrDefault(e => e.Id == listEntryId1);
        Assert.That(updatedEntry, Is.Not.Null);
        Assert.That(updatedEntry.IsStarred, Is.EqualTo(entryToUpdateParameter.IsStarred));
        Assert.That(updatedEntry.Rating, Is.EqualTo(entryToUpdateParameter.Rating));
        Assert.That(updatedEntry.Desc, Is.EqualTo(entryToUpdateParameter.Desc));
        Assert.That(updatedEntry.CompletionStatus, Is.EqualTo(completionStatus));
        Assert.That(updatedEntry.GameDistributions, Is.EquivalentTo([gameDistribution]));
        Assert.That(updatedEntry.Platforms, Is.EquivalentTo([platform]));
    }

    [Test]
    public async Task UpdateListEntries_ListDoesNotExists_ReturnsListNotFoundError()
    {
        //ARRANGE
        var parameters = _fixture.Create<UpdateListEntriesParameter>();

        var userManagerService = new ListsService(_logger, _dbContextFactory, _yglDatabaseAndDtoMapper);

        //ACT
        var result = await userManagerService.UpdateListEntries(parameters);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListNotFound));
        _logger.ReceivedLog(LogLevel.Information, $"No lists found for id '{parameters.ListId}' and user '{parameters.UserInformation.UserId}'.");
    }

    #endregion
}
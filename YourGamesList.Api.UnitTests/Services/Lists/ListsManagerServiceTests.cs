using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using YourGamesList.Api.Model;
using YourGamesList.Api.Services.Lists;
using YourGamesList.Api.Services.Lists.Model;
using YourGamesList.Contracts;
using YourGamesList.Contracts.Dto;
using YourGamesList.Database;
using YourGamesList.Database.Entity;

namespace YourGamesList.Api.UnitTests.Services.Lists;

public class ListsManagerServiceTests
{
    private ILogger<ListsManagerService> _logger = null!;
    private YourGamesListDbContext _dbContext = null!;
    private FakeTimeProvider _timeProvider = null!;

    [SetUp]
    public void SetUp()
    {
        _logger = Substitute.For<ILogger<ListsManagerService>>();
        _timeProvider = new FakeTimeProvider();

        var options = new DbContextOptionsBuilder<YourGamesListDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new YourGamesListDbContext(options);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task GetSelf_UserExists_ReturnsTheirListsAndEntries()
    {
        var user = CreateUser();
        var list = new GamesList
        {
            CreatedAt = DateTimeOffset.UtcNow,
            Editable = true,
            IsPrivate = true,
            Name = "My list",
            User = user
        };
        list.GamesListEntries.Add(new GamesListEntry
        {
            CreatedAt = DateTimeOffset.UtcNow,
            CompletionStatus = CompletionStatus.Played,
            IgdbId = 123
        });
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().GetSelf(new GetSelfListParams
        {
            UserInformation = CreateUserInformation(user.Id)
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Has.Count.EqualTo(1));
        Assert.That(result.Value[0].Id, Is.EqualTo(list.Id));
        Assert.That(result.Value[0].Name, Is.EqualTo("My list"));
        Assert.That(result.Value[0].GameListEntries.Single().Id, Is.EqualTo(list.GamesListEntries.Single().Id));
        Assert.That(result.Value[0].GameListEntries.Single().GamesListId, Is.EqualTo(list.Id));
        Assert.That(result.Value[0].GameListEntries.Single().IgdbId, Is.EqualTo(123));
    }

    [Test]
    public async Task GetSelf_UserDoesNotExist_ReturnsUserNotFound()
    {
        var result = await CreateService().GetSelf(new GetSelfListParams
        {
            UserInformation = CreateUserInformation(Guid.NewGuid())
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.UserNotFound));
    }

    [Test]
    public async Task Search_WithOwnerAndListName_UsesCaseInsensitiveSubstringAndAnd()
    {
        var owner = CreateUser();
        owner.Username = "GameOwner";
        var matchingList = CreateList(owner, "My Games");
        matchingList.IsPrivate = false;
        var wrongOwnerList = CreateList(CreateUser(), "My Games");
        wrongOwnerList.IsPrivate = false;
        var wrongNameList = CreateList(owner, "My Books");
        wrongNameList.IsPrivate = false;
        _dbContext.GamesLists.AddRange(matchingList, wrongOwnerList, wrongNameList);
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().Search(new SearchListsParams
        {
            UserInformation = CreateUserInformation(Guid.NewGuid()),
            OwnerName = "OWNER",
            ListName = "gAm"
        });

        Assert.That(result.Select(list => list.Id), Is.EquivalentTo([matchingList.Id]));
    }

    [Test]
    public async Task Search_ExcludesOtherUsersPrivateListsAndIncludesPublicLists()
    {
        var owner = CreateUser();
        owner.Username = "Owner";
        var privateList = CreateList(owner, "Private collection");
        var publicList = CreateList(owner, "Public collection");
        publicList.IsPrivate = false;
        _dbContext.GamesLists.AddRange(privateList, publicList);
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().Search(new SearchListsParams
        {
            UserInformation = CreateUserInformation(Guid.NewGuid()),
            OwnerName = "owner"
        });

        Assert.That(result.Select(list => list.Id), Is.EquivalentTo([publicList.Id]));
    }

    [Test]
    public async Task Search_OwnerCanFindPrivateLists_AndControlsEntryInclusion()
    {
        var owner = CreateUser();
        owner.Username = "Owner";
        var list = CreateList(owner, "Games");
        CreateEntry(list, 123);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();

        var searchParams = new SearchListsParams
        {
            UserInformation = CreateUserInformation(owner.Id),
            OwnerName = "owner"
        };
        var withoutEntries = await CreateService().Search(searchParams);
        var withEntries = await CreateService().Search(new SearchListsParams
        {
            UserInformation = searchParams.UserInformation,
            OwnerName = searchParams.OwnerName,
            IncludeEntries = true
        });

        Assert.That(withoutEntries.Single().GameListEntries, Is.Empty);
        Assert.That(withEntries.Single().GameListEntries, Has.Count.EqualTo(1));
        Assert.That(withEntries.Single().GameListEntries[0].IgdbId, Is.EqualTo(123));
    }

    [Test]
    public async Task Create_NewName_CreatesPrivateListAndReturnsId()
    {
        var user = CreateUser();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        var createdAt = DateTimeOffset.UtcNow;
        _timeProvider.SetUtcNow(createdAt);

        var result = await CreateService().Create(new CreateListParams
        {
            UserInformation = CreateUserInformation(user.Id),
            Name = "Collection"
        });

        Assert.That(result.IsSuccess, Is.True);
        var list = await _dbContext.GamesLists.SingleAsync();
        Assert.That(result.Value, Is.EqualTo(list.Id));
        Assert.That(list.UserId, Is.EqualTo(user.Id));
        Assert.That(list.Name, Is.EqualTo("Collection"));
        Assert.That(list.IsPrivate, Is.True);
        Assert.That(list.Editable, Is.True);
        Assert.That(list.CreatedAt, Is.EqualTo(createdAt));
    }

    [Test]
    public async Task Create_ExistingName_ReturnsAlreadyExists()
    {
        var user = CreateUser();
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(CreateList(user, "Collection"));
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().Create(new CreateListParams
        {
            UserInformation = CreateUserInformation(user.Id),
            Name = "Collection"
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListAlreadyExists));
    }

    [Test]
    public async Task Delete_OwnedDeletableList_RemovesIt()
    {
        var user = CreateUser();
        var list = CreateList(user, "Collection");
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().Delete(new DeleteListParams
        {
            UserInformation = CreateUserInformation(user.Id),
            Id = list.Id
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(await _dbContext.GamesLists.AnyAsync(), Is.False);
    }

    [Test]
    public async Task Delete_ListOwnedByAnotherUser_ReturnsNotFound()
    {
        var user = CreateUser();
        var list = CreateList(user, "Collection");
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().Delete(new DeleteListParams
        {
            UserInformation = CreateUserInformation(Guid.NewGuid()),
            Id = list.Id
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListNotFound));
        Assert.That(await _dbContext.GamesLists.AnyAsync(), Is.True);
    }

    [Test]
    public async Task Delete_ProtectedList_ReturnsCannotBeDeleted()
    {
        var user = CreateUser();
        var list = CreateList(user, "Wishlist", editable: false);
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().Delete(new DeleteListParams
        {
            UserInformation = CreateUserInformation(user.Id),
            Id = list.Id
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListCannotBeDeleted));
        Assert.That(await _dbContext.GamesLists.AnyAsync(), Is.True);
    }

    [Test]
    public async Task Update_EditableList_UpdatesSuppliedMetadataAndModifiedAt()
    {
        var user = CreateUser();
        var list = CreateList(user, "Collection");
        list.Description = "Before";
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();
        var modifiedAt = DateTimeOffset.UtcNow.AddMinutes(1);
        _timeProvider.SetUtcNow(modifiedAt);

        var result = await CreateService().Update(new UpdateListParams
        {
            UserInformation = CreateUserInformation(user.Id),
            Id = list.Id,
            Name = "Updated",
            HasDescription = true,
            Description = null,
            IsPrivate = false
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(list.Name, Is.EqualTo("Updated"));
        Assert.That(list.Description, Is.Null);
        Assert.That(list.IsPrivate, Is.False);
        Assert.That(list.ModifiedAt, Is.EqualTo(modifiedAt));
    }

    [Test]
    public async Task Update_OmittedFields_LeavesValuesUnchanged()
    {
        var user = CreateUser();
        var list = CreateList(user, "Collection");
        list.Description = "Keep this";
        list.ModifiedAt = DateTimeOffset.UtcNow;
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();
        var originalModifiedAt = list.ModifiedAt;

        var result = await CreateService().Update(new UpdateListParams
        {
            UserInformation = CreateUserInformation(user.Id),
            Id = list.Id
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(list.Name, Is.EqualTo("Collection"));
        Assert.That(list.Description, Is.EqualTo("Keep this"));
        Assert.That(list.IsPrivate, Is.True);
        Assert.That(list.ModifiedAt, Is.EqualTo(originalModifiedAt));
    }

    [Test]
    public async Task Update_NonEditableList_ReturnsCannotBeEdited()
    {
        var user = CreateUser();
        var list = CreateList(user, "Wishlist", editable: false);
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().Update(new UpdateListParams
        {
            UserInformation = CreateUserInformation(user.Id),
            Id = list.Id,
            Name = "New name"
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListCannotBeEdited));
        Assert.That(list.Name, Is.EqualTo("Wishlist"));
    }

    [Test]
    public async Task Update_DuplicateNameForSameUser_ReturnsAlreadyExists()
    {
        var user = CreateUser();
        var list = CreateList(user, "Collection");
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.AddRange(list, CreateList(user, "Other"));
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().Update(new UpdateListParams
        {
            UserInformation = CreateUserInformation(user.Id),
            Id = list.Id,
            Name = "Other"
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListAlreadyExists));
        Assert.That(list.Name, Is.EqualTo("Collection"));
    }

    [Test]
    public async Task Update_ListOwnedByAnotherUser_ReturnsNotFound()
    {
        var user = CreateUser();
        var list = CreateList(user, "Collection");
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().Update(new UpdateListParams
        {
            UserInformation = CreateUserInformation(Guid.NewGuid()),
            Id = list.Id,
            Name = "Changed"
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListNotFound));
        Assert.That(list.Name, Is.EqualTo("Collection"));
    }

    [Test]
    public async Task AddGameListEntry_OwnedList_CreatesEntryAndReturnsId()
    {
        var user = CreateUser();
        var list = CreateList(user, "Wishlist", editable: false);
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();
        var createdAt = DateTimeOffset.UtcNow.AddMinutes(1);
        _timeProvider.SetUtcNow(createdAt);

        var result = await CreateService().AddGameListEntry(new AddGameListEntryParams
        {
            UserInformation = CreateUserInformation(user.Id),
            GamesListId = list.Id,
            CompletionStatus = CompletionStatusDto.Played,
            IgdbId = 123,
            Description = "A note",
            Rating = 8
        });

        var entry = await _dbContext.GameListEntries.SingleAsync();
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(entry.Id));
        Assert.That(entry.GamesListId, Is.EqualTo(list.Id));
        Assert.That(entry.IgdbId, Is.EqualTo(123));
        Assert.That(entry.CompletionStatus, Is.EqualTo(CompletionStatus.Played));
        Assert.That(entry.Description, Is.EqualTo("A note"));
        Assert.That(entry.Rating, Is.EqualTo(8));
        Assert.That(entry.CreatedAt, Is.EqualTo(createdAt));
    }

    [Test]
    public async Task AddGameListEntry_DuplicateIgdbId_ReturnsAlreadyExists()
    {
        var user = CreateUser();
        var list = CreateList(user, "Collection");
        CreateEntry(list, 123);
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().AddGameListEntry(new AddGameListEntryParams
        {
            UserInformation = CreateUserInformation(user.Id),
            GamesListId = list.Id,
            CompletionStatus = CompletionStatusDto.Played,
            IgdbId = 123
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.GameListEntryAlreadyExists));
        Assert.That(await _dbContext.GameListEntries.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task AddGameListEntry_ListOwnedByAnotherUser_ReturnsListNotFound()
    {
        var user = CreateUser();
        var list = CreateList(user, "Collection");
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().AddGameListEntry(new AddGameListEntryParams
        {
            UserInformation = CreateUserInformation(Guid.NewGuid()),
            GamesListId = list.Id,
            CompletionStatus = CompletionStatusDto.Played,
            IgdbId = 123
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListNotFound));
        Assert.That(await _dbContext.GameListEntries.AnyAsync(), Is.False);
    }

    [Test]
    public async Task RemoveGameListEntry_OwnedEntry_RemovesEntryFromNonEditableList()
    {
        var user = CreateUser();
        var list = CreateList(user, "Wishlist", editable: false);
        var entry = CreateEntry(list, 123);
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().RemoveGameListEntry(new RemoveGameListEntryParams
        {
            UserInformation = CreateUserInformation(user.Id),
            GamesListId = list.Id,
            Id = entry.Id
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(await _dbContext.GameListEntries.AnyAsync(), Is.False);
    }

    [Test]
    public async Task RemoveGameListEntry_EntryOwnedByAnotherUser_ReturnsNotFound()
    {
        var user = CreateUser();
        var list = CreateList(user, "Collection");
        var entry = CreateEntry(list, 123);
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().RemoveGameListEntry(new RemoveGameListEntryParams
        {
            UserInformation = CreateUserInformation(Guid.NewGuid()),
            GamesListId = list.Id,
            Id = entry.Id
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.GameListEntryNotFound));
        Assert.That(await _dbContext.GameListEntries.AnyAsync(), Is.True);
    }

    [Test]
    public async Task RemoveGameListEntry_EntryInDifferentList_ReturnsNotFound()
    {
        var user = CreateUser();
        var list = CreateList(user, "Collection");
        var differentList = CreateList(user, "Other");
        var entry = CreateEntry(list, 123);
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.AddRange(list, differentList);
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().RemoveGameListEntry(new RemoveGameListEntryParams
        {
            UserInformation = CreateUserInformation(user.Id),
            GamesListId = differentList.Id,
            Id = entry.Id
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.GameListEntryNotFound));
        Assert.That(await _dbContext.GameListEntries.AnyAsync(), Is.True);
    }

    [Test]
    public async Task UpdateGameListEntry_EditableEntry_UpdatesSuppliedFieldsAndModifiedAt()
    {
        var user = CreateUser();
        var list = CreateList(user, "Collection");
        var entry = CreateEntry(list, 123);
        entry.Description = "Before";
        entry.Rating = 5;
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();
        var modifiedAt = DateTimeOffset.UtcNow.AddMinutes(1);
        _timeProvider.SetUtcNow(modifiedAt);

        var result = await CreateService().UpdateGameListEntry(new UpdateGameListEntryParams
        {
            UserInformation = CreateUserInformation(user.Id),
            GamesListId = list.Id,
            Id = entry.Id,
            CompletionStatus = Optional<CompletionStatusDto?>.Some(CompletionStatusDto.Finished),
            Description = Optional<string?>.Some(null)
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(entry.CompletionStatus, Is.EqualTo(CompletionStatus.Finished));
        Assert.That(entry.Description, Is.Null);
        Assert.That(entry.Rating, Is.EqualTo(5));
        Assert.That(entry.ModifiedAt, Is.EqualTo(modifiedAt));
    }

    [Test]
    public async Task UpdateGameListEntry_NonEditableList_ReturnsCannotBeEdited()
    {
        var user = CreateUser();
        var list = CreateList(user, "Wishlist", editable: false);
        var entry = CreateEntry(list, 123);
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().UpdateGameListEntry(new UpdateGameListEntryParams
        {
            UserInformation = CreateUserInformation(user.Id),
            GamesListId = list.Id,
            Id = entry.Id,
            Rating = Optional<byte?>.Some(8)
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListCannotBeEdited));
        Assert.That(entry.Rating, Is.Null);
    }

    [Test]
    public async Task UpdateGameListEntry_OmittedFields_DoesNotChangeEntry()
    {
        var user = CreateUser();
        var list = CreateList(user, "Collection");
        var entry = CreateEntry(list, 123);
        entry.ModifiedAt = DateTimeOffset.UtcNow;
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();
        var originalModifiedAt = entry.ModifiedAt;

        var result = await CreateService().UpdateGameListEntry(new UpdateGameListEntryParams
        {
            UserInformation = CreateUserInformation(user.Id),
            GamesListId = list.Id,
            Id = entry.Id
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(entry.CompletionStatus, Is.EqualTo(CompletionStatus.JustTried));
        Assert.That(entry.Description, Is.Null);
        Assert.That(entry.Rating, Is.Null);
        Assert.That(entry.ModifiedAt, Is.EqualTo(originalModifiedAt));
    }

    [Test]
    public async Task UpdateGameListEntry_EntryOwnedByAnotherUser_ReturnsNotFound()
    {
        var user = CreateUser();
        var list = CreateList(user, "Collection");
        var entry = CreateEntry(list, 123);
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().UpdateGameListEntry(new UpdateGameListEntryParams
        {
            UserInformation = CreateUserInformation(Guid.NewGuid()),
            GamesListId = list.Id,
            Id = entry.Id,
            Rating = Optional<byte?>.Some(8)
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.GameListEntryNotFound));
        Assert.That(entry.Rating, Is.Null);
    }

    [Test]
    public async Task UpdateGameListEntry_EntryInDifferentList_ReturnsNotFound()
    {
        var user = CreateUser();
        var list = CreateList(user, "Collection");
        var differentList = CreateList(user, "Other");
        var entry = CreateEntry(list, 123);
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.AddRange(list, differentList);
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().UpdateGameListEntry(new UpdateGameListEntryParams
        {
            UserInformation = CreateUserInformation(user.Id),
            GamesListId = differentList.Id,
            Id = entry.Id,
            Rating = Optional<byte?>.Some(8)
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.GameListEntryNotFound));
        Assert.That(entry.Rating, Is.Null);
    }

    private ListsManagerService CreateService() => new(_logger, _dbContext, _timeProvider);

    private static User CreateUser() => new()
    {
        Id = Guid.NewGuid(),
        CreatedAt = DateTimeOffset.UtcNow,
        DateOfBirth = new DateOnly(2000, 1, 1),
        Email = $"{Guid.NewGuid():N}@example.com",
        PasswordHash = "hashed-password",
        UserStatus = UserStatus.Active,
        Username = Guid.NewGuid().ToString("N")
    };

    private static GamesList CreateList(User user, string name, bool editable = true) => new()
    {
        Id = Guid.NewGuid(),
        CreatedAt = DateTimeOffset.UtcNow,
        Editable = editable,
        IsPrivate = true,
        Name = name,
        User = user
    };

    private static GamesListEntry CreateEntry(GamesList list, long igdbId)
    {
        var entry = new GamesListEntry
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            CompletionStatus = CompletionStatus.JustTried,
            IgdbId = igdbId,
            GamesListId = list.Id
        };
        list.GamesListEntries.Add(entry);
        return entry;
    }

    private static UserInformationToken CreateUserInformation(Guid userId) => new()
    {
        RawToken = "token",
        Username = "testuser",
        UserId = userId
    };
}

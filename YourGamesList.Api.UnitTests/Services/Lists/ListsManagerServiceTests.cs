using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Services.Lists;
using YourGamesList.Api.Services.Lists.Model;
using YourGamesList.Contracts;
using YourGamesList.Contracts.Dto;
using YourGamesList.Contracts.Requests.Lists;
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

    #region GameList

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
            NormalizedName = "my list",
            User = user
        };
        var entry = new GamesListEntry
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            CompletionStatus = CompletionStatus.Played,
            IgdbId = 123,
            GamesListId = list.Id
        };
        entry.OwnershipInfo.Add(new OwnershipInfo
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            Platform = Platform.PC,
            GameDistribution = GameDistribution.Steam,
            GameListEntryId = entry.Id
        });
        list.GamesListEntries.Add(entry);
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
        Assert.That(
            result.Value[0].GameListEntries.Single().OwnershipInfos.Single().Id,
            Is.EqualTo(entry.OwnershipInfo.Single().Id));
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
        var owner = CreateUser("GameOwner");
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
        var owner = CreateUser("Owner");
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
        var owner = CreateUser("Owner");
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

        var result = await CreateService().Create(new AddListParams
        {
            UserInformation = CreateUserInformation(user.Id),
            Name = "Collection"
        });

        Assert.That(result.IsSuccess, Is.True);
        var list = await _dbContext.GamesLists.SingleAsync();
        Assert.That(result.Value, Is.EqualTo(list.Id));
        Assert.That(list.UserId, Is.EqualTo(user.Id));
        Assert.That(list.Name, Is.EqualTo("Collection"));
        Assert.That(list.NormalizedName, Is.EqualTo("collection"));
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

        var result = await CreateService().Create(new AddListParams
        {
            UserInformation = CreateUserInformation(user.Id),
            Name = "Collection"
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListAlreadyExists));
    }

    [Test]
    public async Task Create_NameDiffersOnlyByCase_ReturnsAlreadyExists()
    {
        var user = CreateUser();
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(CreateList(user, "Collection"));
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().Create(new AddListParams
        {
            UserInformation = CreateUserInformation(user.Id),
            Name = "COLLECTION"
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListAlreadyExists));
        Assert.That(await _dbContext.GamesLists.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task Create_NameUsedByAnotherUser_CreatesList()
    {
        var user = CreateUser();
        var otherUser = CreateUser();
        _dbContext.Users.AddRange(user, otherUser);
        _dbContext.GamesLists.Add(CreateList(otherUser, "Collection"));
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().Create(new AddListParams
        {
            UserInformation = CreateUserInformation(user.Id),
            Name = "collection"
        });

        Assert.That(result.IsSuccess, Is.True);
        var list = await _dbContext.GamesLists.SingleAsync(l => l.UserId == user.Id);
        Assert.That(list.Name, Is.EqualTo("collection"));
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
        Assert.That(list.NormalizedName, Is.EqualTo("updated"));
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
    public async Task Update_NonEditableList_DescriptionChange_ReturnsCannotBeEdited()
    {
        var user = CreateUser();
        var list = CreateList(user, "Custom protected", editable: false);
        list.Description = "Before";
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().Update(new UpdateListParams
        {
            UserInformation = CreateUserInformation(user.Id),
            Id = list.Id,
            HasDescription = true,
            Description = "After"
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListCannotBeEdited));
        Assert.That(list.Description, Is.EqualTo("Before"));
    }

    [Test]
    public async Task Update_NonEditableList_PrivacyChange_ReturnsCannotBeEdited()
    {
        var user = CreateUser();
        var list = CreateList(user, "Custom protected", editable: false);
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().Update(new UpdateListParams
        {
            UserInformation = CreateUserInformation(user.Id),
            Id = list.Id,
            IsPrivate = !list.IsPrivate
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListCannotBeEdited));
        Assert.That(list.IsPrivate, Is.True);
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
    public async Task Update_NameDiffersOnlyByCaseFromAnotherList_ReturnsAlreadyExists()
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
            Name = "OTHER"
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.ListAlreadyExists));
        Assert.That(list.Name, Is.EqualTo("Collection"));
    }

    [Test]
    public async Task Update_OnlyCaseOfOwnNameChanged_UpdatesName()
    {
        var user = CreateUser();
        var list = CreateList(user, "Collection");
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();

        var result = await CreateService().Update(new UpdateListParams
        {
            UserInformation = CreateUserInformation(user.Id),
            Id = list.Id,
            Name = "COLLECTION"
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(list.Name, Is.EqualTo("COLLECTION"));
        Assert.That(list.NormalizedName, Is.EqualTo("collection"));
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

    #endregion

    #region GameListEntry

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
    public async Task UpdateGameListEntry_NonEditableList_UpdatesEntry()
    {
        var user = CreateUser();
        var list = CreateList(user, "Wishlist", editable: false);
        var entry = CreateEntry(list, 123);
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
            Rating = Optional<byte?>.Some(8)
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(entry.CompletionStatus, Is.EqualTo(CompletionStatus.Finished));
        Assert.That(entry.Rating, Is.EqualTo(8));
        Assert.That(entry.ModifiedAt, Is.EqualTo(modifiedAt));
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

    #endregion

    #region OwnershipInfo

    [Test]
    public void AddOwnershipInfoValidator_RequiresValidPlatformAndGameDistribution()
    {
        //ARRANGE
        var request = new AddOwnershipInfoRequest
        {
            UserInformation = CreateUserInformation(Guid.NewGuid()),
            ListId = Guid.NewGuid(),
            EntryId = Guid.NewGuid(),
            Body = new AddOwnershipInfoRequestBody
            {
                Platform = (PlatformDto)999,
                GameDistribution = (GameDistributionDto)999
            }
        };

        //ACT
        var result = new AddOwnershipInfoRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.Platform);
        result.ShouldHaveValidationErrorFor(x => x.Body.GameDistribution);
    }

    [Test]
    public async Task AddOwnershipInfo_OwnedEntryInNonEditableList_AddsMultipleRecords()
    {
        //ARRANGE
        var user = CreateUser();
        var list = CreateList(user, "Wishlist", editable: false);
        var entry = CreateEntry(list, 123);
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();
        var createdAt = DateTimeOffset.UtcNow.AddMinutes(1);
        _timeProvider.SetUtcNow(createdAt);
        var parameters = new AddOwnershipInfoParams
        {
            UserInformation = CreateUserInformation(user.Id),
            GamesListId = list.Id,
            GameListEntryId = entry.Id,
            Platform = PlatformDto.PC,
            GameDistribution = GameDistributionDto.Steam,
            IsLegit = true,
            WasEmulated = true,
            EmulatedOn = EmulatorDto.RPCS3
        };

        //ACT
        var firstResult = await CreateService().AddOwnershipInfo(parameters);
        var secondResult = await CreateService().AddOwnershipInfo(parameters);

        //ASSERT
        var ownershipInfos = await _dbContext.OwnershipInfos.ToListAsync();
        Assert.That(firstResult.IsSuccess, Is.True);
        Assert.That(secondResult.IsSuccess, Is.True);
        Assert.That(firstResult.Value, Is.Not.EqualTo(secondResult.Value));
        Assert.That(ownershipInfos, Has.Count.EqualTo(2));
        Assert.That(ownershipInfos[0].GameListEntryId, Is.EqualTo(entry.Id));
        Assert.That(ownershipInfos[0].Platform, Is.EqualTo(Platform.PC));
        Assert.That(ownershipInfos[0].GameDistribution, Is.EqualTo(GameDistribution.Steam));
        Assert.That(ownershipInfos[0].IsLegit, Is.True);
        Assert.That(ownershipInfos[0].WasEmulated, Is.True);
        Assert.That(ownershipInfos[0].EmulatedOn, Is.EqualTo(Emulator.RPCS3));
        Assert.That(ownershipInfos[0].CreatedAt, Is.EqualTo(createdAt));
    }

    [Test]
    public async Task AddOwnershipInfo_EntryNotOwnedByCaller_ReturnsNotFound()
    {
        //ARRANGE
        var user = CreateUser();
        var list = CreateList(user, "Wishlist");
        var entry = CreateEntry(list, 123);
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();

        //ACT
        var result = await CreateService().AddOwnershipInfo(new AddOwnershipInfoParams
        {
            UserInformation = CreateUserInformation(Guid.NewGuid()),
            GamesListId = list.Id,
            GameListEntryId = entry.Id,
            Platform = PlatformDto.PC,
            GameDistribution = GameDistributionDto.Steam
        });

        //ASSERT
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.GameListEntryNotFound));
        Assert.That(await _dbContext.OwnershipInfos.AnyAsync(), Is.False);
    }

    [Test]
    public void UpdateOwnershipInfoValidator_RejectsNullNonNullableFieldsAndUndefinedEnums()
    {
        //ARRANGE
        var request = new UpdateOwnershipInfoRequest
        {
            UserInformation = CreateUserInformation(Guid.NewGuid()),
            ListId = Guid.NewGuid(),
            EntryId = Guid.NewGuid(),
            OwnershipInfoId = Guid.NewGuid(),
            Body = new UpdateOwnershipInfoRequestBody
            {
                Platform = Optional<PlatformDto?>.Some(null),
                GameDistribution = Optional<GameDistributionDto?>.Some((GameDistributionDto)999),
                WasEmulated = Optional<bool?>.Some(null),
                EmulatedOn = Optional<EmulatorDto?>.Some((EmulatorDto)999)
            }
        };

        //ACT
        var result = new UpdateOwnershipInfoRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.Body.Platform);
        result.ShouldHaveValidationErrorFor(x => x.Body.GameDistribution);
        result.ShouldHaveValidationErrorFor(x => x.Body.WasEmulated);
        result.ShouldHaveValidationErrorFor(x => x.Body.EmulatedOn);
    }

    [Test]
    public void UpdateOwnershipInfoValidator_AllowsExplicitNullForNullableFields()
    {
        //ARRANGE
        var request = new UpdateOwnershipInfoRequest
        {
            UserInformation = CreateUserInformation(Guid.NewGuid()),
            ListId = Guid.NewGuid(),
            EntryId = Guid.NewGuid(),
            OwnershipInfoId = Guid.NewGuid(),
            Body = new UpdateOwnershipInfoRequestBody
            {
                IsLegit = Optional<bool?>.Some(null),
                EmulatedOn = Optional<EmulatorDto?>.Some(null)
            }
        };

        //ACT
        var result = new UpdateOwnershipInfoRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task UpdateOwnershipInfo_OwnedEntryInNonEditableList_UpdatesSuppliedFields()
    {
        //ARRANGE
        var user = CreateUser();
        var list = CreateList(user, "Wishlist", editable: false);
        var entry = CreateEntry(list, 123);
        var ownershipInfo = CreateOwnershipInfo(entry);
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();
        var modifiedAt = DateTimeOffset.UtcNow.AddMinutes(1);
        _timeProvider.SetUtcNow(modifiedAt);

        //ACT
        var result = await CreateService().UpdateOwnershipInfo(new UpdateOwnershipInfoParams
        {
            UserInformation = CreateUserInformation(user.Id),
            GamesListId = list.Id,
            GameListEntryId = entry.Id,
            OwnershipInfoId = ownershipInfo.Id,
            IsLegit = Optional<bool?>.Some(null),
            Platform = Optional<PlatformDto?>.Some(PlatformDto.PlayStation5),
            GameDistribution = Optional<GameDistributionDto?>.Some(GameDistributionDto.Physical),
            WasEmulated = Optional<bool?>.Some(false),
            EmulatedOn = Optional<EmulatorDto?>.Some(null)
        });

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(ownershipInfo.IsLegit, Is.Null);
        Assert.That(ownershipInfo.Platform, Is.EqualTo(Platform.PlayStation5));
        Assert.That(ownershipInfo.GameDistribution, Is.EqualTo(GameDistribution.Physical));
        Assert.That(ownershipInfo.WasEmulated, Is.False);
        Assert.That(ownershipInfo.EmulatedOn, Is.Null);
        Assert.That(ownershipInfo.ModifiedAt, Is.EqualTo(modifiedAt));
    }

    [Test]
    public async Task UpdateOwnershipInfo_EntryNotOwnedByCaller_ReturnsNotFound()
    {
        //ARRANGE
        var user = CreateUser();
        var list = CreateList(user, "Wishlist", editable: false);
        var entry = CreateEntry(list, 123);
        var ownershipInfo = CreateOwnershipInfo(entry);
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();

        //ACT
        var result = await CreateService().UpdateOwnershipInfo(new UpdateOwnershipInfoParams
        {
            UserInformation = CreateUserInformation(Guid.NewGuid()),
            GamesListId = list.Id,
            GameListEntryId = entry.Id,
            OwnershipInfoId = ownershipInfo.Id,
            WasEmulated = Optional<bool?>.Some(true)
        });

        //ASSERT
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.GameListEntryNotFound));
        Assert.That(ownershipInfo.WasEmulated, Is.True);
    }

    [Test]
    public void RemoveOwnershipInfoValidator_RequiresAllRouteIdsAndUser()
    {
        //ARRANGE
        var request = new RemoveOwnershipInfoRequest
        {
            UserInformation = null!,
            ListId = Guid.Empty,
            EntryId = Guid.Empty,
            OwnershipInfoId = Guid.Empty
        };

        //ACT
        var result = new RemoveOwnershipInfoRequestValidator().TestValidate(request);

        //ASSERT
        Assert.That(result.IsValid, Is.False);
        result.ShouldHaveValidationErrorFor(x => x.ListId);
        result.ShouldHaveValidationErrorFor(x => x.EntryId);
        result.ShouldHaveValidationErrorFor(x => x.OwnershipInfoId);
        result.ShouldHaveValidationErrorFor(x => x.UserInformation);
    }

    [Test]
    public async Task RemoveOwnershipInfo_OwnedEntry_RemovesSpecifiedRecord()
    {
        //ARRANGE
        var user = CreateUser();
        var list = CreateList(user, "Wishlist", editable: false);
        var entry = CreateEntry(list, 123);
        var ownershipInfo = CreateOwnershipInfo(entry);
        CreateOwnershipInfo(entry);
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();

        //ACT
        var result = await CreateService().RemoveOwnershipInfo(new RemoveOwnershipInfoParams
        {
            UserInformation = CreateUserInformation(user.Id),
            GamesListId = list.Id,
            GameListEntryId = entry.Id,
            OwnershipInfoId = ownershipInfo.Id
        });

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(await _dbContext.OwnershipInfos.CountAsync(), Is.EqualTo(1));
        Assert.That(await _dbContext.OwnershipInfos.AnyAsync(info => info.Id == ownershipInfo.Id), Is.False);
    }

    [Test]
    public async Task RemoveOwnershipInfo_EntryNotOwnedByCaller_ReturnsNotFound()
    {
        //ARRANGE
        var user = CreateUser();
        var list = CreateList(user, "Wishlist", editable: false);
        var entry = CreateEntry(list, 123);
        var ownershipInfo = CreateOwnershipInfo(entry);
        _dbContext.Users.Add(user);
        _dbContext.GamesLists.Add(list);
        await _dbContext.SaveChangesAsync();

        //ACT
        var result = await CreateService().RemoveOwnershipInfo(new RemoveOwnershipInfoParams
        {
            UserInformation = CreateUserInformation(Guid.NewGuid()),
            GamesListId = list.Id,
            GameListEntryId = entry.Id,
            OwnershipInfoId = ownershipInfo.Id
        });

        //ASSERT
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ListsError.GameListEntryNotFound));
        Assert.That(await _dbContext.OwnershipInfos.AnyAsync(info => info.Id == ownershipInfo.Id), Is.True);
    }

    private static OwnershipInfo CreateOwnershipInfo(GamesListEntry entry)
    {
        var ownershipInfo = new OwnershipInfo
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            Platform = Platform.PC,
            GameDistribution = GameDistribution.Steam,
            WasEmulated = true,
            EmulatedOn = Emulator.RPCS3,
            IsLegit = true,
            GameListEntryId = entry.Id
        };
        entry.OwnershipInfo.Add(ownershipInfo);
        return ownershipInfo;
    }

    #endregion

    private ListsManagerService CreateService() => new(_logger, _dbContext, _timeProvider);

    private static User CreateUser(string? username = null)
    {
        username ??= Guid.NewGuid().ToString("N");
        return new User
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            DateOfBirth = new DateOnly(2000, 1, 1),
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = "hashed-password",
            UserStatus = UserStatus.Active,
            Username = username,
            NormalizedUsername = username.ToLowerInvariant()
        };
    }

    private static GamesList CreateList(User user, string name, bool editable = true) => new()
    {
        Id = Guid.NewGuid(),
        CreatedAt = DateTimeOffset.UtcNow,
        Editable = editable,
        IsPrivate = true,
        Name = name,
        NormalizedName = name.ToLowerInvariant(),
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
        NormalizedUsername = "testuser",
        UserId = userId
    };
}

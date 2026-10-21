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

    private static UserInformationToken CreateUserInformation(Guid userId) => new()
    {
        RawToken = "token",
        Username = "testuser",
        UserId = userId
    };
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Controllers.Lists;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Services;
using YourGamesList.Api.Services.Lists;
using YourGamesList.Api.Services.Lists.Model;
using YourGamesList.Common;
using YourGamesList.Contracts;
using YourGamesList.Contracts.Dto;
using YourGamesList.Contracts.Requests.Lists;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.UnitTests.Controllers.Lists;

public class ListsControllerTests
{
    private IFixture _fixture = null!;
    private ILogger<ListsController> _logger = null!;
    private IListsManagerService _listsManagerService = null!;
    private IDomainErrorMapper _domainErrorMapper = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<ListsController>>();
        _listsManagerService = Substitute.For<IListsManagerService>();
        _domainErrorMapper = Substitute.For<IDomainErrorMapper>();
    }

    [Test]
    public async Task GetSelf_Success_ReturnsListsAndPassesUserInformation()
    {
        //ARRANGE
        var userInformation = CreateUserInformation();
        var lists = new List<GamesListDto>();
        _listsManagerService.GetSelf(Arg.Any<GetSelfListParams>()).Returns(CombinedResult<List<GamesListDto>, ListsError>.Success(lists));

        //ACT
        var result = await CreateController().GetSelf(new GetSelfListRequest
        {
            UserInformation = userInformation
        });

        //ASSERT
        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(objectResult.Value, Is.EqualTo(lists));
        await _listsManagerService.Received(1).GetSelf(Arg.Is<GetSelfListParams>(parameters => parameters.UserInformation == userInformation));
    }

    [Test]
    public async Task GetSelf_UserNotFound_ReturnsNotFound()
    {
        //ARRANGE
        _listsManagerService.GetSelf(Arg.Any<GetSelfListParams>()).Returns(CombinedResult<List<GamesListDto>, ListsError>.Failure(ListsError.UserNotFound));

        //ACT
        var result = await CreateController().GetSelf(new GetSelfListRequest
        {
            UserInformation = CreateUserInformation()
        });

        //ASSERT
        var statusCodeResult = result as IStatusCodeActionResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
    }

    [Test]
    public async Task Search_Success_ReturnsListsAndPassesFilters()
    {
        //ARRANGE
        var userInformation = CreateUserInformation();
        var lists = new List<GamesListDto>();
        _listsManagerService.Search(Arg.Any<SearchListsParams>()).Returns(lists);

        //ACT
        var result = await CreateController().Search(new SearchListsRequest
        {
            UserInformation = userInformation,
            OwnerName = "  owner ",
            ListName = " games ",
            IncludeEntries = true
        });

        //ASSERT
        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(objectResult?.Value, Is.SameAs(lists));
        await _listsManagerService.Received(1).Search(Arg.Is<SearchListsParams>(parameters =>
            parameters.UserInformation == userInformation &&
            parameters.OwnerName == "owner" &&
            parameters.ListName == "games" &&
            parameters.IncludeEntries));
    }

    [Test]
    public async Task Create_Success_ReturnsIdAndPassesRequestData()
    {
        //ARRANGE
        var userInformation = CreateUserInformation();
        var listId = Guid.NewGuid();
        var listName = _fixture.Create<string>();
        _listsManagerService.Create(Arg.Any<CreateListParams>()).Returns(CombinedResult<Guid, ListsError>.Success(listId));

        //ACT
        var result = await CreateController().Add(new CreateListRequest
        {
            UserInformation = userInformation,
            Body = new CreateListRequestBody(listName)
        });

        //ASSERT
        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(objectResult.Value, Is.EqualTo(listId));
        await _listsManagerService.Received(1).Create(
            Arg.Is<CreateListParams>(parameters =>
                parameters.UserInformation == userInformation &&
                parameters.Name == listName));
    }

    [Test]
    public async Task Create_ListAlreadyExists_ReturnsConflict()
    {
        //ARRANGE
        var listName = _fixture.Create<string>();
        var error = _fixture.Create<Error>();
        _domainErrorMapper.Map(ListsError.ListAlreadyExists).Returns(error);
        _listsManagerService.Create(Arg.Any<CreateListParams>()).Returns(CombinedResult<Guid, ListsError>.Failure(ListsError.ListAlreadyExists));

        //ACT
        var result = await CreateController().Add(new CreateListRequest
        {
            UserInformation = CreateUserInformation(),
            Body = new CreateListRequestBody(listName)
        });

        //ASSERT
        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
        var errorResponse = objectResult.Value as ErrorResponse;
        Assert.That(errorResponse, Is.Not.Null);
        var errors = errorResponse.Errors.ToArray();
        Assert.That(errors, Has.Length.EqualTo(1));
        Assert.That(errors[0], Is.SameAs(error));
    }

    [Test]
    public async Task Create_UserNotFound_ReturnsNotFound()
    {
        //ARRANGE
        var listName = _fixture.Create<string>();
        _listsManagerService.Create(Arg.Any<CreateListParams>()).Returns(CombinedResult<Guid, ListsError>.Failure(ListsError.UserNotFound));

        //ACT
        var result = await CreateController().Add(new CreateListRequest
        {
            UserInformation = CreateUserInformation(),
            Body = new CreateListRequestBody(listName)
        });

        //ASSERT
        var statusCodeResult = result as IStatusCodeActionResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
    }

    [Test]
    public async Task Delete_Success_ReturnsNoContentAndPassesRequestData()
    {
        //ARRANGE
        var userInformation = CreateUserInformation();
        var listId = Guid.NewGuid();
        _listsManagerService.Delete(Arg.Any<DeleteListParams>()).Returns(ErrorResult<ListsError>.Success());

        //ACT
        var result = await CreateController().Remove(new DeleteListRequest
        {
            UserInformation = userInformation,
            ListId = listId
        });

        //ASSERT
        var statusCodeResult = result as IStatusCodeActionResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status204NoContent));
        await _listsManagerService.Received(1).Delete(
            Arg.Is<DeleteListParams>(parameters =>
                parameters.UserInformation == userInformation &&
                parameters.Id == listId));
    }

    [Test]
    public async Task Delete_ListNotFound_ReturnsNotFound()
    {
        //ARRANGE
        _listsManagerService.Delete(Arg.Any<DeleteListParams>()).Returns(ErrorResult<ListsError>.Failure(ListsError.ListNotFound));

        //ACT
        var result = await CreateController().Remove(new DeleteListRequest
        {
            UserInformation = CreateUserInformation(),
            ListId = Guid.NewGuid()
        });

        //ASSERT
        var statusCodeResult = result as IStatusCodeActionResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
    }

    [Test]
    public async Task Delete_ListCannotBeDeleted_ReturnsConflict()
    {
        //ARRANGE
        var error = _fixture.Create<Error>();
        _domainErrorMapper.Map(ListsError.ListCannotBeDeleted).Returns(error);
        _listsManagerService.Delete(Arg.Any<DeleteListParams>()).Returns(ErrorResult<ListsError>.Failure(ListsError.ListCannotBeDeleted));

        //ACT
        var result = await CreateController().Remove(new DeleteListRequest
        {
            UserInformation = CreateUserInformation(),
            ListId = Guid.NewGuid()
        });

        //ASSERT
        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
        var errorResponse = objectResult.Value as ErrorResponse;
        Assert.That(errorResponse, Is.Not.Null);
        var errors = errorResponse.Errors.ToArray();
        Assert.That(errors, Has.Length.EqualTo(1));
        Assert.That(errors[0], Is.SameAs(error));
    }

    [Test]
    public async Task Update_Success_ReturnsNoContentAndPassesRequestData()
    {
        //ARRANGE
        var userInformation = CreateUserInformation();
        var listId = Guid.NewGuid();
        _listsManagerService.Update(Arg.Any<UpdateListParams>()).Returns(ErrorResult<ListsError>.Success());

        //ACT
        var result = await CreateController().Update(new UpdateListRequest
        {
            UserInformation = userInformation,
            ListId = listId,
            Body = new UpdateListRequestBody
            {
                Name = Optional<string?>.Some(" Updated "),
                Description = Optional<string?>.Some(null),
                IsPrivate = Optional<bool?>.Some(false)
            }
        });

        //ASSERT
        var statusCodeResult = result as IStatusCodeActionResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult?.StatusCode, Is.EqualTo(StatusCodes.Status204NoContent));
        await _listsManagerService.Received(1).Update(
            Arg.Is<UpdateListParams>(parameters =>
                parameters.UserInformation == userInformation &&
                parameters.Id == listId &&
                parameters.Name == "Updated" &&
                parameters.HasDescription &&
                parameters.Description == null &&
                parameters.IsPrivate == false));
    }

    [Test]
    public async Task Update_ListCannotBeEdited_ReturnsConflict()
    {
        //ARRANGE
        var error = _fixture.Create<Error>();
        _domainErrorMapper.Map(ListsError.ListCannotBeEdited).Returns(error);
        _listsManagerService.Update(Arg.Any<UpdateListParams>())
            .Returns(ErrorResult<ListsError>.Failure(ListsError.ListCannotBeEdited));

        //ACT
        var result = await CreateController().Update(new UpdateListRequest
        {
            UserInformation = CreateUserInformation(),
            ListId = Guid.NewGuid(),
            Body = new UpdateListRequestBody()
        });

        //ASSERT
        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult?.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
        var errorResponse = objectResult?.Value as ErrorResponse;
        Assert.That(errorResponse, Is.Not.Null);
        Assert.That(errorResponse?.Errors.Single(), Is.SameAs(error));
    }

    [Test]
    public async Task AddGameListEntry_Success_ReturnsEntryId()
    {
        //ARRANGE
        var userInformation = CreateUserInformation();
        var listId = Guid.NewGuid();
        var entryId = Guid.NewGuid();
        _listsManagerService.AddGameListEntry(Arg.Any<AddGameListEntryParams>())
            .Returns(CombinedResult<Guid, ListsError>.Success(entryId));

        //ACT
        var result = await CreateController().AddGameListEntry(new AddGameListEntryRequest
        {
            UserInformation = userInformation,
            ListId = listId,
            Body = new AddGameListEntryRequestBody
            {
                CompletionStatus = CompletionStatusDto.Played,
                IgdbId = 123
            }
        });

        //ASSERT
        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(objectResult?.Value, Is.EqualTo(entryId));
        await _listsManagerService.Received(1).AddGameListEntry(
            Arg.Is<AddGameListEntryParams>(parameters =>
                parameters.UserInformation == userInformation &&
                parameters.GamesListId == listId &&
                parameters.CompletionStatus == CompletionStatusDto.Played &&
                parameters.IgdbId == 123));
    }

    [Test]
    public async Task AddGameListEntry_Duplicate_ReturnsConflict()
    {
        //ARRANGE
        var error = _fixture.Create<Error>();
        _domainErrorMapper.Map(ListsError.GameListEntryAlreadyExists).Returns(error);
        _listsManagerService.AddGameListEntry(Arg.Any<AddGameListEntryParams>())
            .Returns(CombinedResult<Guid, ListsError>.Failure(ListsError.GameListEntryAlreadyExists));

        //ACT
        var result = await CreateController().AddGameListEntry(new AddGameListEntryRequest
        {
            UserInformation = CreateUserInformation(),
            ListId = Guid.NewGuid(),
            Body = new AddGameListEntryRequestBody
            {
                CompletionStatus = CompletionStatusDto.Played,
                IgdbId = 123
            }
        });

        //ASSERT
        var objectResult = result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
        Assert.That(((ErrorResponse)objectResult!.Value!).Errors.Single(), Is.SameAs(error));
    }

    [Test]
    public async Task RemoveGameListEntry_Success_ReturnsNoContentAndPassesEntryId()
    {
        //ARRANGE
        var userInformation = CreateUserInformation();
        var listId = Guid.NewGuid();
        var entryId = Guid.NewGuid();
        _listsManagerService.RemoveGameListEntry(Arg.Any<RemoveGameListEntryParams>())
            .Returns(ErrorResult<ListsError>.Success());

        //ACT
        var result = await CreateController().RemoveGameListEntry(new RemoveGameListEntryRequest
        {
            UserInformation = userInformation,
            ListId = listId,
            EntryId = entryId
        });

        //ASSERT
        Assert.That((result as IStatusCodeActionResult)?.StatusCode, Is.EqualTo(StatusCodes.Status204NoContent));
        await _listsManagerService.Received(1).RemoveGameListEntry(
            Arg.Is<RemoveGameListEntryParams>(parameters =>
                parameters.UserInformation == userInformation &&
                parameters.GamesListId == listId &&
                parameters.Id == entryId));
    }

    [Test]
    public async Task UpdateGameListEntry_Success_PassesPartialFields()
    {
        //ARRANGE
        var userInformation = CreateUserInformation();
        var listId = Guid.NewGuid();
        var entryId = Guid.NewGuid();
        _listsManagerService.UpdateGameListEntry(Arg.Any<UpdateGameListEntryParams>())
            .Returns(ErrorResult<ListsError>.Success());

        //ACT
        var result = await CreateController().UpdateGameListEntry(new UpdateGameListEntryRequest
        {
            UserInformation = userInformation,
            ListId = listId,
            EntryId = entryId,
            Body = new UpdateGameListEntryRequestBody
            {
                Description = Optional<string?>.Some(null),
                Rating = Optional<byte?>.Some(10)
            }
        });

        //ASSERT
        Assert.That((result as IStatusCodeActionResult)?.StatusCode, Is.EqualTo(StatusCodes.Status204NoContent));
        await _listsManagerService.Received(1).UpdateGameListEntry(
            Arg.Is<UpdateGameListEntryParams>(parameters =>
                parameters.UserInformation == userInformation &&
                parameters.GamesListId == listId &&
                parameters.Id == entryId &&
                !parameters.CompletionStatus.HasValue &&
                parameters.Description.HasValue &&
                parameters.Description.Value == null &&
                parameters.Rating.Value == 10));
    }

    private ListsController CreateController()
    {
        return new ListsController(_logger, _listsManagerService, _domainErrorMapper);
    }

    private UserInformationToken CreateUserInformation()
    {
        return new UserInformationToken()
        {
            RawToken = _fixture.Create<string>(),
            Username = _fixture.Create<string>(),
            UserId = Guid.NewGuid()
        };
    }

}
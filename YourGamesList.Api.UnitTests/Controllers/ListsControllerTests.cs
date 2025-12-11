using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Controllers;
using YourGamesList.Api.Controllers.Lists;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Services.ModelMappers;
using YourGamesList.Api.Services.Ygl.Lists;
using YourGamesList.Api.Services.Ygl.Lists.Model;
using YourGamesList.Common;
using YourGamesList.Contracts.Dto;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Controllers;

public class ListsControllerTests
{
    private IFixture _fixture;
    private ILogger<ListsController> _logger;
    private IRequestToParametersMapper _requestToParametersMapper;
    private IListsService _listsService;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<ListsController>>();
        _requestToParametersMapper = Substitute.For<IRequestToParametersMapper>();
        _listsService = Substitute.For<IListsService>();
    }

    #region CreateList

    [Test]
    public async Task CreateList_SuccessScenario()
    {
        //ARRANGE
        var expectedResValue = _fixture.Create<Guid>();
        var request = _fixture.Create<CreateListRequest>();
        _listsService.CreateList(request.UserInformation, request.Body.ListName, request.Body.Description)
            .Returns(CombinedResult<Guid, ListsError>.Success(expectedResValue));

        var controller = new ListsController(_logger, _requestToParametersMapper, _listsService);

        //ACT
        var res = await controller.CreateList(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(objectResult.Value, Is.EqualTo(expectedResValue));
        await _listsService.Received(1).CreateList(request.UserInformation, request.Body.ListName, request.Body.Description);
        _logger.ReceivedLog(LogLevel.Information, $"Requested to create list '{request.Body.ListName}' for user '{request.UserInformation.UserId}'");
    }

    [Test]
    public async Task CreateList_OnErrorListAlreadyExists_ReturnsStatus409Conflict()
    {
        //ARRANGE
        var expectedError = ListsError.ListAlreadyExists;
        var request = _fixture.Create<CreateListRequest>();
        _listsService.CreateList(request.UserInformation, request.Body.ListName, request.Body.Description)
            .Returns(CombinedResult<Guid, ListsError>.Failure(expectedError));

        var controller = new ListsController(_logger, _requestToParametersMapper, _listsService);

        //ACT
        var res = await controller.CreateList(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
        await _listsService.Received(1).CreateList(request.UserInformation, request.Body.ListName, request.Body.Description);
        _logger.ReceivedLog(LogLevel.Information, $"Requested to create list '{request.Body.ListName}' for user '{request.UserInformation.UserId}'");
    }

    #endregion

    #region SearchLists

    [Test]
    public async Task SearchLists_SuccessScenario()
    {
        //ARRANGE
        var expectedResValue = _fixture.CreateMany<GamesListDto>().ToList();
        var request = _fixture.Create<SearchListsRequest>();
        var parameters = _fixture.Create<SearchListsParameters>();
        _requestToParametersMapper.Map(request).Returns(parameters);
        _listsService.SearchLists(parameters).Returns(CombinedResult<List<GamesListDto>, ListsError>.Success(expectedResValue));

        var controller = new ListsController(_logger, _requestToParametersMapper, _listsService);

        //ACT
        var res = await controller.SearchLists(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(objectResult.Value, Is.EqualTo(expectedResValue));
        await _listsService.Received(1).SearchLists(parameters);
        _logger.ReceivedLog(LogLevel.Information, $"Requested to search list name: '{request.Body.ListName}' user: '{request.Body.UserName}'");
    }

    [Test]
    public async Task SearchLists_OnErrorListNotFound_ReturnsStatus404NotFound()
    {
        //ARRANGE
        var expectedError = ListsError.ListNotFound;
        var request = _fixture.Create<SearchListsRequest>();
        var parameters = _fixture.Create<SearchListsParameters>();
        _requestToParametersMapper.Map(request).Returns(parameters);
        _listsService.SearchLists(parameters).Returns(CombinedResult<List<GamesListDto>, ListsError>.Failure(expectedError));

        var controller = new ListsController(_logger, _requestToParametersMapper, _listsService);

        //ACT
        var res = await controller.SearchLists(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        await _listsService.Received(1).SearchLists(parameters);
        _logger.ReceivedLog(LogLevel.Information, $"Requested to search list name: '{request.Body.ListName}' user: '{request.Body.UserName}'");
    }

    #endregion

    #region GetSelfLists

    [Test]
    public async Task GetSelfLists_SuccessScenario()
    {
        //ARRANGE
        var expectedResValue = _fixture.CreateMany<GamesListDto>().ToList();
        var request = _fixture.Create<GetSelfListsRequest>();
        _listsService.GetSelfLists(request.UserInformation, request.IncludeGames.Value)
            .Returns(CombinedResult<List<GamesListDto>, ListsError>.Success(expectedResValue));

        var controller = new ListsController(_logger, _requestToParametersMapper, _listsService);

        //ACT
        var res = await controller.GetSelfLists(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(objectResult.Value, Is.EqualTo(expectedResValue));
        await _listsService.Received(1).GetSelfLists(request.UserInformation, request.IncludeGames.Value);
        _logger.ReceivedLog(LogLevel.Information, $"Requested to get self list for user: '{request.UserInformation.UserId}'");
    }

    [Test]
    public async Task GetSelfLists_OnErrorListNotFound_ReturnsStatus404NotFound()
    {
        //ARRANGE
        var expectedError = ListsError.ListNotFound;
        var request = _fixture.Create<GetSelfListsRequest>();
        _listsService.GetSelfLists(request.UserInformation, request.IncludeGames.Value)
            .Returns(CombinedResult<List<GamesListDto>, ListsError>.Failure(expectedError));

        var controller = new ListsController(_logger, _requestToParametersMapper, _listsService);

        //ACT
        var res = await controller.GetSelfLists(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        await _listsService.Received(1).GetSelfLists(request.UserInformation, request.IncludeGames.Value);
        _logger.ReceivedLog(LogLevel.Information, $"Requested to get self list for user: '{request.UserInformation.UserId}'");
    }

    #endregion

    #region UpdateList

    [Test]
    public async Task UpdateList_SuccessScenario()
    {
        //ARRANGE
        var expectedResValue = _fixture.Create<Guid>();
        var request = _fixture.Create<UpdateListRequest>();
        var parameters = _fixture.Create<UpdateListParameters>();
        _requestToParametersMapper.Map(request).Returns(parameters);
        _listsService.UpdateList(parameters).Returns(CombinedResult<Guid, ListsError>.Success(expectedResValue));

        var controller = new ListsController(_logger, _requestToParametersMapper, _listsService);

        //ACT
        var res = await controller.UpdateList(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(objectResult.Value, Is.EqualTo(expectedResValue));
        await _listsService.Received(1).UpdateList(parameters);
        _logger.ReceivedLog(LogLevel.Information, $"Requested to update list '{request.Body.ListId}' for user '{request.UserInformation.UserId}'");
    }

    [Test]
    public async Task UpdateList_OnErrorListNotFound_ReturnsStatus404NotFound()
    {
        //ARRANGE
        var expectedError = ListsError.ListNotFound;
        var request = _fixture.Create<UpdateListRequest>();
        var parameters = _fixture.Create<UpdateListParameters>();
        _requestToParametersMapper.Map(request).Returns(parameters);
        _listsService.UpdateList(parameters).Returns(CombinedResult<Guid, ListsError>.Failure(expectedError));

        var controller = new ListsController(_logger, _requestToParametersMapper, _listsService);

        //ACT
        var res = await controller.UpdateList(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        await _listsService.Received(1).UpdateList(parameters);
        _logger.ReceivedLog(LogLevel.Information, $"Requested to update list '{request.Body.ListId}' for user '{request.UserInformation.UserId}'");
    }

    [Test]
    public async Task UpdateList_OnErrorListAlreadyExists_ReturnsStatus409Conflict()
    {
        //ARRANGE
        var expectedError = ListsError.ListAlreadyExists;
        var request = _fixture.Create<UpdateListRequest>();
        var parameters = _fixture.Create<UpdateListParameters>();
        _requestToParametersMapper.Map(request).Returns(parameters);
        _listsService.UpdateList(parameters).Returns(CombinedResult<Guid, ListsError>.Failure(expectedError));

        var controller = new ListsController(_logger, _requestToParametersMapper, _listsService);

        //ACT
        var res = await controller.UpdateList(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
        await _listsService.Received(1).UpdateList(parameters);
        _logger.ReceivedLog(LogLevel.Information, $"Requested to update list '{request.Body.ListId}' for user '{request.UserInformation.UserId}'");
    }

    #endregion

    #region DeleteList

    [Test]
    public async Task DeleteList_SuccessScenario()
    {
        //ARRANGE
        var expectedResValue = _fixture.Create<Guid>();
        var request = _fixture.Create<DeleteListRequest>();
        _listsService.DeleteList(request.UserInformation, request.ListId).Returns(CombinedResult<Guid, ListsError>.Success(expectedResValue));

        var controller = new ListsController(_logger, _requestToParametersMapper, _listsService);

        //ACT
        var res = await controller.DeleteList(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(objectResult.Value, Is.EqualTo(expectedResValue));
        await _listsService.Received(1).DeleteList(request.UserInformation, request.ListId);
        _logger.ReceivedLog(LogLevel.Information, $"Requested to delete list '{request.ListId}' for user '{request.UserInformation.UserId}'");
    }

    [Test]
    public async Task DeleteList_OnErrorListNotFound_ReturnsStatus404NotFound()
    {
        //ARRANGE
        var expectedError = ListsError.ListNotFound;
        var request = _fixture.Create<DeleteListRequest>();
        _listsService.DeleteList(request.UserInformation, request.ListId).Returns(CombinedResult<Guid, ListsError>.Failure(expectedError));

        var controller = new ListsController(_logger, _requestToParametersMapper, _listsService);

        //ACT
        var res = await controller.DeleteList(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        await _listsService.Received(1).DeleteList(request.UserInformation, request.ListId);
        _logger.ReceivedLog(LogLevel.Information, $"Requested to delete list '{request.ListId}' for user '{request.UserInformation.UserId}'");
    }

    [Test]
    public async Task DeleteList_OnErrorListHardLocked_ReturnsStatus423Locked()
    {
        //ARRANGE
        var expectedError = ListsError.ListHardLocked;
        var request = _fixture.Create<DeleteListRequest>();
        _listsService.DeleteList(request.UserInformation, request.ListId).Returns(CombinedResult<Guid, ListsError>.Failure(expectedError));

        var controller = new ListsController(_logger, _requestToParametersMapper, _listsService);

        //ACT
        var res = await controller.DeleteList(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status423Locked));
        await _listsService.Received(1).DeleteList(request.UserInformation, request.ListId);
        _logger.ReceivedLog(LogLevel.Information, $"Requested to delete list '{request.ListId}' for user '{request.UserInformation.UserId}'");
    }

    #endregion

    #region AddListEntries

    [Test]
    public async Task AddListEntries_SuccessScenario()
    {
        //ARRANGE
        var expectedResValue = _fixture.CreateMany<Guid>().ToList();
        var request = _fixture.Create<AddEntriesToListRequest>();
        var parameters = _fixture.Create<AddEntriesToListParameter>();
        _requestToParametersMapper.Map(request).Returns(parameters);
        _listsService.AddListEntries(parameters).Returns(CombinedResult<List<Guid>, ListsError>.Success(expectedResValue));

        var controller = new ListsController(_logger, _requestToParametersMapper, _listsService);

        //ACT
        var res = await controller.AddListEntries(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(objectResult.Value, Is.EqualTo(expectedResValue));
        await _listsService.Received(1).AddListEntries(parameters);
        _logger.ReceivedLog(LogLevel.Information, $"Requested to add entries to list '{request.Body.ListId}' for user '{request.UserInformation.UserId}'");
    }

    [Test]
    public async Task AddListEntries_OnErrorListNotFound_ReturnsStatus404NotFound()
    {
        //ARRANGE
        var expectedError = ListsError.ListNotFound;
        var request = _fixture.Create<AddEntriesToListRequest>();
        var parameters = _fixture.Create<AddEntriesToListParameter>();
        _requestToParametersMapper.Map(request).Returns(parameters);
        _listsService.AddListEntries(parameters).Returns(CombinedResult<List<Guid>, ListsError>.Failure(expectedError));

        var controller = new ListsController(_logger, _requestToParametersMapper, _listsService);

        //ACT
        var res = await controller.AddListEntries(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        await _listsService.Received(1).AddListEntries(parameters);
        _logger.ReceivedLog(LogLevel.Information, $"Requested to add entries to list '{request.Body.ListId}' for user '{request.UserInformation.UserId}'");
    }

    #endregion

    #region DeleteListEntries

    [Test]
    public async Task DeleteListEntries_SuccessScenario()
    {
        //ARRANGE
        var expectedResValue = _fixture.CreateMany<Guid>().ToList();
        var request = _fixture.Create<DeleteListEntriesRequest>();
        var parameters = _fixture.Create<DeleteListEntriesParameter>();
        _requestToParametersMapper.Map(request).Returns(parameters);
        _listsService.DeleteListEntries(parameters).Returns(CombinedResult<List<Guid>, ListsError>.Success(expectedResValue));

        var controller = new ListsController(_logger, _requestToParametersMapper, _listsService);

        //ACT
        var res = await controller.DeleteListEntries(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(objectResult.Value, Is.EqualTo(expectedResValue));
        await _listsService.Received(1).DeleteListEntries(parameters);
        _logger.ReceivedLog(LogLevel.Information, $"Requested to delete entries from list '{request.Body.ListId}' for user '{request.UserInformation.UserId}'");
    }

    [Test]
    public async Task DeleteListEntries_OnErrorListNotFound_ReturnsStatus404NotFound()
    {
        //ARRANGE
        var expectedError = ListsError.ListNotFound;
        var request = _fixture.Create<DeleteListEntriesRequest>();
        var parameters = _fixture.Create<DeleteListEntriesParameter>();
        _requestToParametersMapper.Map(request).Returns(parameters);
        _listsService.DeleteListEntries(parameters).Returns(CombinedResult<List<Guid>, ListsError>.Failure(expectedError));

        var controller = new ListsController(_logger, _requestToParametersMapper, _listsService);

        //ACT
        var res = await controller.DeleteListEntries(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        await _listsService.Received(1).DeleteListEntries(parameters);
        _logger.ReceivedLog(LogLevel.Information, $"Requested to delete entries from list '{request.Body.ListId}' for user '{request.UserInformation.UserId}'");
    }

    #endregion

    #region UpdateListEntries

    [Test]
    public async Task UpdateListEntries_SuccessScenario()
    {
        //ARRANGE
        var expectedResValue = _fixture.CreateMany<Guid>().ToList();
        var request = _fixture.Create<UpdateListEntriesRequest>();
        var parameters = _fixture.Create<UpdateListEntriesParameter>();
        _requestToParametersMapper.Map(request).Returns(parameters);
        _listsService.UpdateListEntries(parameters).Returns(CombinedResult<List<Guid>, ListsError>.Success(expectedResValue));

        var controller = new ListsController(_logger, _requestToParametersMapper, _listsService);

        //ACT
        var res = await controller.UpdateListEntries(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(objectResult.Value, Is.EqualTo(expectedResValue));
        await _listsService.Received(1).UpdateListEntries(parameters);
        _logger.ReceivedLog(LogLevel.Information, $"Requested to update entries in list '{request.Body.ListId}' for user '{request.UserInformation.UserId}'");
    }

    [Test]
    public async Task UpdateListEntries_OnErrorListNotFound_ReturnsStatus404NotFound()
    {
        //ARRANGE
        var expectedError = ListsError.ListNotFound;
        var request = _fixture.Create<UpdateListEntriesRequest>();
        var parameters = _fixture.Create<UpdateListEntriesParameter>();
        _requestToParametersMapper.Map(request).Returns(parameters);
        _listsService.UpdateListEntries(parameters).Returns(CombinedResult<List<Guid>, ListsError>.Failure(expectedError));

        var controller = new ListsController(_logger, _requestToParametersMapper, _listsService);

        //ACT
        var res = await controller.UpdateListEntries(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        await _listsService.Received(1).UpdateListEntries(parameters);
        _logger.ReceivedLog(LogLevel.Information, $"Requested to update entries in list '{request.Body.ListId}' for user '{request.UserInformation.UserId}'");
    }

    #endregion
}
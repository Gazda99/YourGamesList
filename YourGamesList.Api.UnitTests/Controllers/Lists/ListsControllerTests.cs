using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Controllers.Lists;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Services.Lists;
using YourGamesList.Api.Services.Lists.Model;
using YourGamesList.Common;
using YourGamesList.Contracts.Dto;
using YourGamesList.Contracts.Requests.Lists;

namespace YourGamesList.Api.UnitTests.Controllers.Lists;

public class ListsControllerTests
{
    private ILogger<ListsController> _logger = null!;
    private IListsManagerService _listsManagerService = null!;

    [SetUp]
    public void SetUp()
    {
        _logger = Substitute.For<ILogger<ListsController>>();
        _listsManagerService = Substitute.For<IListsManagerService>();
    }

    [Test]
    public async Task GetSelf_Success_ReturnsListsAndPassesUserInformation()
    {
        var userInformation = CreateUserInformation();
        var lists = new List<GamesListDto>();
        _listsManagerService.GetSelf(Arg.Any<GetSelfListParams>())
            .Returns(CombinedResult<List<GamesListDto>, ListsError>.Success(lists));

        var result = await CreateController().GetSelf(new GetSelfListRequest
        {
            UserInformation = userInformation
        });

        AssertObjectResult(result, StatusCodes.Status200OK, lists);
        await _listsManagerService.Received(1).GetSelf(
            Arg.Is<GetSelfListParams>(parameters => parameters.UserInformation == userInformation));
    }

    [TestCase(ListsError.UserNotFound, StatusCodes.Status404NotFound)]
    [TestCase(ListsError.InternalServerError, StatusCodes.Status400BadRequest)]
    [TestCase(ListsError.ListAlreadyExists, StatusCodes.Status500InternalServerError)]
    public async Task GetSelf_Failure_ReturnsMappedStatusCode(ListsError error, int expectedStatusCode)
    {
        _listsManagerService.GetSelf(Arg.Any<GetSelfListParams>())
            .Returns(CombinedResult<List<GamesListDto>, ListsError>.Failure(error));

        var result = await CreateController().GetSelf(new GetSelfListRequest
        {
            UserInformation = CreateUserInformation()
        });

        AssertStatusCode(result, expectedStatusCode);
    }

    [Test]
    public async Task Create_Success_ReturnsIdAndPassesRequestData()
    {
        var userInformation = CreateUserInformation();
        var listId = Guid.NewGuid();
        _listsManagerService.Create(Arg.Any<CreateListParams>())
            .Returns(CombinedResult<Guid, ListsError>.Success(listId));

        var result = await CreateController().Create(new CreateListRequest
        {
            UserInformation = userInformation,
            Body = new CreateListRequestBody("My games")
        });

        AssertObjectResult(result, StatusCodes.Status200OK, listId);
        await _listsManagerService.Received(1).Create(
            Arg.Is<CreateListParams>(parameters =>
                parameters.UserInformation == userInformation &&
                parameters.Name == "My games"));
    }

    [TestCase(ListsError.ListAlreadyExists, StatusCodes.Status409Conflict)]
    [TestCase(ListsError.ListNameTooLong, StatusCodes.Status400BadRequest)]
    [TestCase(ListsError.UserNotFound, StatusCodes.Status404NotFound)]
    [TestCase(ListsError.InternalServerError, StatusCodes.Status400BadRequest)]
    [TestCase(ListsError.ListNotFound, StatusCodes.Status500InternalServerError)]
    public async Task Create_Failure_ReturnsMappedStatusCode(ListsError error, int expectedStatusCode)
    {
        _listsManagerService.Create(Arg.Any<CreateListParams>())
            .Returns(CombinedResult<Guid, ListsError>.Failure(error));

        var result = await CreateController().Create(new CreateListRequest
        {
            UserInformation = CreateUserInformation(),
            Body = new CreateListRequestBody("My games")
        });

        AssertStatusCode(result, expectedStatusCode);
    }

    [Test]
    public async Task Delete_Success_ReturnsNoContentAndPassesRequestData()
    {
        var userInformation = CreateUserInformation();
        var listId = Guid.NewGuid();
        _listsManagerService.Delete(Arg.Any<DeleteListParams>())
            .Returns(ErrorResult<ListsError>.Success());

        var result = await CreateController().Delete(new DeleteListRequest
        {
            UserInformation = userInformation,
            Body = new DeleteListRequestBody(listId)
        });

        AssertStatusCode(result, StatusCodes.Status204NoContent);
        await _listsManagerService.Received(1).Delete(
            Arg.Is<DeleteListParams>(parameters =>
                parameters.UserInformation == userInformation &&
                parameters.Id == listId));
    }

    [TestCase(ListsError.ListNotFound, StatusCodes.Status404NotFound)]
    [TestCase(ListsError.ListCannotBeDeleted, StatusCodes.Status409Conflict)]
    [TestCase(ListsError.InternalServerError, StatusCodes.Status400BadRequest)]
    [TestCase(ListsError.ListAlreadyExists, StatusCodes.Status500InternalServerError)]
    public async Task Delete_Failure_ReturnsMappedStatusCode(ListsError error, int expectedStatusCode)
    {
        _listsManagerService.Delete(Arg.Any<DeleteListParams>())
            .Returns(ErrorResult<ListsError>.Failure(error));

        var result = await CreateController().Delete(new DeleteListRequest
        {
            UserInformation = CreateUserInformation(),
            Body = new DeleteListRequestBody(Guid.NewGuid())
        });

        AssertStatusCode(result, expectedStatusCode);
    }

    private ListsController CreateController() => new(_logger, _listsManagerService);

    private static UserInformationToken CreateUserInformation() => new()
    {
        RawToken = "token",
        Username = "testuser",
        UserId = Guid.NewGuid()
    };

    private static void AssertObjectResult(IActionResult result, int expectedStatusCode, object expectedValue)
    {
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(expectedStatusCode));
        Assert.That(objectResult.Value, Is.EqualTo(expectedValue));
    }

    private static void AssertStatusCode(IActionResult result, int expectedStatusCode)
    {
        Assert.That(result, Is.InstanceOf<IStatusCodeActionResult>());
        Assert.That(((IStatusCodeActionResult)result).StatusCode, Is.EqualTo(expectedStatusCode));
    }
}

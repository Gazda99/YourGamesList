using System;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Controllers;
using YourGamesList.Api.Model.Requests.SearchIgdbGames;
using YourGamesList.Api.Services.Igdb;
using YourGamesList.Api.Services.Igdb.Model;
using YourGamesList.Common;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Controllers;

public class SearchIgdbGamesControllerTests
{
    private IFixture _fixture;
    private ILogger<SearchIgdbGamesController> _logger;
    private IGamesIgdbService _gamesIgdbService;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<SearchIgdbGamesController>>();
        _gamesIgdbService = Substitute.For<IGamesIgdbService>();
    }

    #region SearchGameByName

    [Test]
    public async Task SearchGameByName_SuccessScecnario()
    {
        //ARRANGE
        var gameName = _fixture.Create<string>();
        var request = new SearchIgdbGameByNameRequest()
        {
            GameName = gameName
        };
        var games = _fixture.Create<IgdbGame[]>();
        _gamesIgdbService.GetGamesByName(gameName).Returns(ValueResult<IgdbGame[]>.Success(games));
        var controller = new SearchIgdbGamesController(_logger, _gamesIgdbService);

        //ACT
        var res = await controller.SearchGameByName(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(objectResult.Value, Is.EquivalentTo(games));
        _logger.ReceivedLog(LogLevel.Information, $"Successfully obtained '{games.Length}' games. Returning 200.");
    }

    [Test]
    public async Task SearchGameByName_OnEmptyGamesList_ReturnsStatus404NotFound()
    {
        //ARRANGE
        var gameName = _fixture.Create<string>();
        var request = new SearchIgdbGameByNameRequest()
        {
            GameName = gameName
        };
        var games = Array.Empty<IgdbGame>();
        _gamesIgdbService.GetGamesByName(gameName).Returns(ValueResult<IgdbGame[]>.Success(games));
        var controller = new SearchIgdbGamesController(_logger, _gamesIgdbService);

        //ACT
        var res = await controller.SearchGameByName(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        Assert.That(objectResult.Value, Is.EquivalentTo(Array.Empty<IgdbGame>()));
        _logger.ReceivedLog(LogLevel.Information, "No games found, returning 404.");
    }

    #endregion

    #region SearchGameByIds

    [Test]
    public async Task SearchGameByIds_SuccessScecnario()
    {
        //ARRANGE
        var gameIds = _fixture.Create<int[]>();
        var request = new SearchIgdbGamesByIdsRequest()
        {
            GameIds = gameIds
        };
        var games = _fixture.Create<IgdbGame[]>();
        _gamesIgdbService.GetGamesByIds(gameIds).Returns(ValueResult<IgdbGame[]>.Success(games));
        var controller = new SearchIgdbGamesController(_logger, _gamesIgdbService);

        //ACT
        var res = await controller.SearchGameByIds(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(objectResult.Value, Is.EquivalentTo(games));
        _logger.ReceivedLog(LogLevel.Information, $"Successfully obtained '{games.Length}' games. Returning 200.");
    }

    [Test]
    public async Task SearchGameByIds_OnEmptyGamesList_ReturnsStatus404NotFound()
    {
        //ARRANGE
        var gameIds = _fixture.Create<int[]>();
        var request = new SearchIgdbGamesByIdsRequest()
        {
            GameIds = gameIds
        };
        var games = Array.Empty<IgdbGame>();
        _gamesIgdbService.GetGamesByIds(gameIds).Returns(ValueResult<IgdbGame[]>.Success(games));
        var controller = new SearchIgdbGamesController(_logger, _gamesIgdbService);

        //ACT
        var res = await controller.SearchGameByIds(request);

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        Assert.That(objectResult.Value, Is.EquivalentTo(Array.Empty<IgdbGame>()));
        _logger.ReceivedLog(LogLevel.Information, "No games found, returning 404.");
    }

    #endregion
}
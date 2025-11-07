using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Services.Igdb;
using YourGamesList.Api.Services.Igdb.Model;
using YourGamesList.Common;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.Igdb;

public class IgdbGamesServiceTests
{
    private const string RequestGameFields = "cover.*,first_release_date,game_type.type,genres.name,id,name,rating_count,storyline,summary,themes.name";

    private IFixture _fixture;
    private ILogger<IgdbGamesService> _logger;
    private IIgdbService _igdbService;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<IgdbGamesService>>();
        _igdbService = Substitute.For<IIgdbService>();
    }

    [Test]
    public async Task GetGamesByName_SuccessfulScenario_ReturnsIgdbGames()
    {
        //ARRANGE
        var gameName = _fixture.Create<string>();
        var games = _fixture.CreateMany<IgdbGame>().ToArray();
        _igdbService.CallIgdb<IgdbGame[]>(IgdbEndpoints.Game, Arg.Is<string>(x =>
                x.Contains(RequestGameFields)
                && x.Contains($"name ~ *\"{gameName}\"*")
                && x.Contains("rating_count desc")
            ))
            .Returns(ValueResult<IgdbGame[]>.Success(games));

        var igdbGameService = new IgdbGamesService(_logger, _igdbService);

        //ACT
        var res = await igdbGameService.GetGamesByName(gameName);

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
        _logger.ReceivedLog(LogLevel.Information, $"Searching for game '{gameName}'");
        await _igdbService.Received(1).CallIgdb<IgdbGame[]>(IgdbEndpoints.Game, Arg.Is<string>(x =>
            x.Contains(RequestGameFields)
            && x.Contains($"name ~ *\"{gameName}\"*")
            && x.Contains("rating_count desc")
        ));
    }

    [Test]
    public async Task GetGamesByIds_SuccessfulScenario_ReturnsIgdbGames()
    {
        //ARRANGE
        var gameIds = _fixture.CreateMany<int>().ToArray();
        var ids = string.Join(",", gameIds);
        var games = _fixture.CreateMany<IgdbGame>().ToArray();
        _igdbService.CallIgdb<IgdbGame[]>(IgdbEndpoints.Game, Arg.Is<string>(x =>
            x.Contains(RequestGameFields)
            && x.Contains($"id = ({ids})")
        )).Returns(ValueResult<IgdbGame[]>.Success(games));

        var igdbGameService = new IgdbGamesService(_logger, _igdbService);

        //ACT
        var res = await igdbGameService.GetGamesByIds(gameIds);

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
        _logger.ReceivedLog(LogLevel.Information, $"Searching for '{gameIds.Length}' games with ids: '{ids}'");
        await _igdbService.Received(1).CallIgdb<IgdbGame[]>(IgdbEndpoints.Game, Arg.Is<string>(x =>
            x.Contains(RequestGameFields)
            && x.Contains($"id = ({ids})")
        ));
    }
}
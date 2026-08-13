using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Services.Igdb;
using YourGamesList.Api.Services.Twitch;
using YourGamesList.Common;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.Igdb;

public class IgdbServiceTests
{
    private IFixture _fixture;
    private ILogger<IgdbService> _logger;
    private IIgdbApi _igdbApi;
    private ITwitchAuthService _twitchAuthService;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<IgdbService>>();
        _igdbApi = Substitute.For<IIgdbApi>();
        _twitchAuthService = Substitute.For<ITwitchAuthService>();
    }

    [Test]
    public async Task CallIgdb_SuccessfulScenario_ReturnsIgdbRetrievedData()
    {
        //ARRANGE
        var endpoint = _fixture.Create<IgdbEndpoint>();
        var query = _fixture.Create<string>();
        var twitchClientId = _fixture.Create<string>();
        var accessToken = _fixture.Create<string>();

        _twitchAuthService.GetClientId().Returns(twitchClientId);
        _twitchAuthService.GetAccessToken().Returns(ValueResult<string>.Success(accessToken));

        var testIgdbData = _fixture.Create<TestIgdbData>();
        var apiResponse = RefitHelper.ApiResponseSubstitute(HttpStatusCode.OK, testIgdbData);
        _igdbApi.Endpoint<TestIgdbData>(endpoint.Endpoint, accessToken, twitchClientId, query).Returns(apiResponse);

        var igdbService = new IgdbService(_logger, _igdbApi, _twitchAuthService);

        //ACT
        var res = await igdbService.CallIgdb<TestIgdbData>(endpoint, query);

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
        Assert.That(res.Value, Is.EqualTo(testIgdbData));
        _logger.ReceivedLog(LogLevel.Information, $"Calling IGDB API Endpoint: '{endpoint.Endpoint}'");
        _logger.ReceivedLog(LogLevel.Information, "Successfully obtained response from IGDB API.");
    }

    [Test]
    public async Task CallIgdb_NotOkResponseFromIgdb_ReturnsFailure()
    {
        //ARRANGE
        var endpoint = _fixture.Create<IgdbEndpoint>();
        var query = _fixture.Create<string>();
        var twitchClientId = _fixture.Create<string>();
        var accessToken = _fixture.Create<string>();

        _twitchAuthService.GetClientId().Returns(twitchClientId);
        _twitchAuthService.GetAccessToken().Returns(ValueResult<string>.Success(accessToken));

        var testIgdbData = _fixture.Create<TestIgdbData>();
        var statusCode = HttpStatusCode.BadRequest;
        var apiResponse = RefitHelper.ApiResponseSubstitute(statusCode, testIgdbData);
        _igdbApi.Endpoint<TestIgdbData>(endpoint.Endpoint, accessToken, twitchClientId, query).Returns(apiResponse);

        var igdbService = new IgdbService(_logger, _igdbApi, _twitchAuthService);

        //ACT
        var res = await igdbService.CallIgdb<TestIgdbData>(endpoint, query);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        _logger.ReceivedLog(LogLevel.Information, $"Calling IGDB API Endpoint: '{endpoint.Endpoint}'");
        _logger.ReceivedLog(LogLevel.Error, $"Unhandled HTTP Status Code '{(int) statusCode}' from IGDB API.");
    }


    [Test]
    public async Task CallIgdb_FailsOnObtainingTwitchAuthToken_ReturnsFailure()
    {
        //ARRANGE
        var endpoint = _fixture.Create<IgdbEndpoint>();
        var query = _fixture.Create<string>();
        var twitchClientId = _fixture.Create<string>();

        _twitchAuthService.GetClientId().Returns(twitchClientId);
        _twitchAuthService.GetAccessToken().Returns(ValueResult<string>.Failure());

        var igdbService = new IgdbService(_logger, _igdbApi, _twitchAuthService);

        //ACT
        var res = await igdbService.CallIgdb<TestIgdbData>(endpoint, query);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        _logger.ReceivedLog(LogLevel.Error, "Getting Twitch access token failed");
    }

    public class TestIgdbData
    {
        public string Id { get; set; }
    }
}
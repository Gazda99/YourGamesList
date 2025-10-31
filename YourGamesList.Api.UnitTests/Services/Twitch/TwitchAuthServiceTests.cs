using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Refit;
using YourGamesList.Api.Services.Twitch;
using YourGamesList.Api.Services.Twitch.Model.Responses;
using YourGamesList.Api.Services.Twitch.Options;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.Twitch;

public class TwitchAuthServiceTests
{
    private IFixture _fixture;
    private ILogger<TwitchAuthService> _logger;
    private ITwitchAuthApi _twitchAuthApi;
    private IOptions<TwitchAuthOptions> _twitchAuthOptions;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<TwitchAuthService>>();
        _twitchAuthApi = Substitute.For<ITwitchAuthApi>();
        _twitchAuthOptions = Substitute.For<IOptions<TwitchAuthOptions>>();
    }

    [Test]
    public void GetClientId_ReturnsClientId()
    {
        //ARRANGE

        var options = _fixture.Create<TwitchAuthOptions>();
        _twitchAuthOptions.Value.Returns(options);

        var twitchAuthService = new TwitchAuthService(_logger, _twitchAuthApi, _twitchAuthOptions);

        //ACT
        var res = twitchAuthService.GetClientId();

        //ASSERT
        Assert.That(res, Is.EquivalentTo(options.ClientId));
    }

    [Test]
    public async Task GetAccessToken_OnSuccess_ReturnsAuthToken()
    {
        //ARRANGE
        var options = _fixture.Create<TwitchAuthOptions>();
        _twitchAuthOptions.Value.Returns(options);

        var twitchAuthResponse = _fixture.Create<TwitchAuthResponse>();
        var apiResponse = Substitute.For<IApiResponse<TwitchAuthResponse>>();
        apiResponse.Content.Returns(twitchAuthResponse);
        apiResponse.StatusCode.Returns(HttpStatusCode.OK);
        _twitchAuthApi.GetAccessToken(Arg.Any<FormUrlEncodedContent>()).Returns(apiResponse);

        var twitchAuthService = new TwitchAuthService(_logger, _twitchAuthApi, _twitchAuthOptions);

        //ACT
        var res = await twitchAuthService.GetAccessToken();

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
        Assert.That(res.Value, Is.EquivalentTo(twitchAuthResponse.AccessToken));
        await _twitchAuthApi.Received(1).GetAccessToken(Arg.Any<FormUrlEncodedContent>());
        _logger.ReceivedLog(LogLevel.Information, "Successfully obtained Twitch access token from Twitch.");
    }

    [Test]
    public async Task GetAccessToken_OnNetworkFailure_ReturnsError()
    {
        //ARRANGE
        var options = _fixture.Create<TwitchAuthOptions>();
        _twitchAuthOptions.Value.Returns(options);

        var twitchAuthResponse = _fixture.Create<TwitchAuthResponse>();
        var apiResponse = Substitute.For<IApiResponse<TwitchAuthResponse>>();
        apiResponse.Content.Returns(twitchAuthResponse);
        apiResponse.StatusCode.Returns(HttpStatusCode.OK);
        _twitchAuthApi.GetAccessToken(Arg.Any<FormUrlEncodedContent>()).ThrowsAsync(new Exception());

        var twitchAuthService = new TwitchAuthService(_logger, _twitchAuthApi, _twitchAuthOptions);

        //ACT
        var res = await twitchAuthService.GetAccessToken();

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        await _twitchAuthApi.Received(1).GetAccessToken(Arg.Any<FormUrlEncodedContent>());
    }

    [Test]
    public async Task GetAccessToken_OnNonOkTwitchResponse_ReturnsError()
    {
        //ARRANGE
        var options = _fixture.Create<TwitchAuthOptions>();
        _twitchAuthOptions.Value.Returns(options);

        var apiResponse = Substitute.For<IApiResponse<TwitchAuthResponse>>();
        const HttpStatusCode status = HttpStatusCode.BadRequest;
        apiResponse.StatusCode.Returns(status);
        _twitchAuthApi.GetAccessToken(Arg.Any<FormUrlEncodedContent>()).Returns(apiResponse);

        var twitchAuthService = new TwitchAuthService(_logger, _twitchAuthApi, _twitchAuthOptions);

        //ACT
        var res = await twitchAuthService.GetAccessToken();

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        await _twitchAuthApi.Received(1).GetAccessToken(Arg.Any<FormUrlEncodedContent>());
        _logger.ReceivedLog(LogLevel.Error, $"Twitch responded with '{status}' status code. Could not obtain Twitch access token.");
    }

    [Test]
    public async Task GetAccessToken_OnEmptyAuthToken_ReturnsError()
    {
        //ARRANGE
        var options = _fixture.Create<TwitchAuthOptions>();
        _twitchAuthOptions.Value.Returns(options);

        var twitchAuthResponse = _fixture
            .Build<TwitchAuthResponse>()
            .With(x => x.AccessToken, (string?) null)
            .Create();
        var apiResponse = Substitute.For<IApiResponse<TwitchAuthResponse>>();
        apiResponse.Content.Returns(twitchAuthResponse);
        apiResponse.StatusCode.Returns(HttpStatusCode.OK);
        _twitchAuthApi.GetAccessToken(Arg.Any<FormUrlEncodedContent>()).Returns(apiResponse);

        var twitchAuthService = new TwitchAuthService(_logger, _twitchAuthApi, _twitchAuthOptions);

        //ACT
        var res = await twitchAuthService.GetAccessToken();

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        await _twitchAuthApi.Received(1).GetAccessToken(Arg.Any<FormUrlEncodedContent>());
        _logger.ReceivedLog(LogLevel.Error, "Obtained Twitch access token is null or empty.");
    }
}
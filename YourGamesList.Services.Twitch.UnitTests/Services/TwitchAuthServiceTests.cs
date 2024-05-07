using System.Net;
using Gazda.HttpMock;
using Lib.ServerTiming;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TddXt.AnyRoot.Builder;
using TddXt.AnyRoot.Strings;
using YourGamesList.Common;
using YourGamesList.Services.Twitch.Exceptions;
using YourGamesList.Services.Twitch.Internal.Model;
using YourGamesList.Services.Twitch.Options;
using YourGamesList.Services.Twitch.Services;

namespace YourGamesList.Services.Twitch.UnitTests.Services;

public class TwitchAuthServiceTests
{
    private const string AuthEndpoint = "/oauth2/token";

    [Test]
    public async Task ObtainAccessToken_Should_GetAccessTokenFromTwitchAPIAndStoreItInCache()
    {
        //GIVEN
        var logger = Substitute.For<ILogger<TwitchAuthService>>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        var accessToken = Any.String();
        var mockedResponse = mockHttpMessageHandler
            .PrepareMockResponse(TwitchResponse(accessToken, (DateTime.UtcNow.AddMonths(2) - DateTime.UtcNow).Seconds))
            .ForMethod(HttpMethod.Post);

        var client = mockHttpMessageHandler.ToHttpClient();
        client.BaseAddress = new Uri("https://id.twitch.tv");
        httpClientFactory.CreateClient("TwitchAuthHttpClient").Returns(client);
        var clientId = Any.String();
        var options = Any.Instance<TwitchAuthOptions>()
            .WithProperty(x => x.TwitchAuthEndpoint, AuthEndpoint)
            .WithProperty(x => x.ClientId, clientId);
        var iOptions = Microsoft.Extensions.Options.Options.Create<TwitchAuthOptions>(options);
        var serverTiming = Substitute.For<IServerTiming>();
        var timeProvider = Substitute.For<TimeProvider>();
        var memoryCache = Substitute.For<IMemoryCache>();

        var twitchAuthService =
            new TwitchAuthService(logger, httpClientFactory, iOptions, serverTiming, timeProvider, memoryCache);

        //WHEN

        var twitchAuthResult = await twitchAuthService.ObtainAccessToken();

        //THEN

        twitchAuthResult.Should().NotBeNull();
        twitchAuthResult.AccessToken.Should().BeEquivalentTo(accessToken);
        mockHttpMessageHandler.CountResponseReturns(mockedResponse).Should().Be(1);
        Received.InOrder(() =>
        {
            memoryCache.TryGetValue(GetCacheKey(clientId), out _);
            logger.LogInformation("Obtained twitch auth data from twitch auth service.");
            memoryCache.CreateEntry(GetCacheKey(clientId));
            logger.LogDebug("Saved twitch auth data in cache.");
        });
    }

    [Test]
    public async Task ObtainAccessToken_Should_ReadAccessTokenFromCache()
    {
        //GIVEN
        var logger = Substitute.For<ILogger<TwitchAuthService>>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var accessToken = Any.String();
        var client = Substitute.For<HttpClient>();
        httpClientFactory.CreateClient("TwitchAuthHttpClient").Returns(client);
        var clientId = Any.String();
        var options = Any.Instance<TwitchAuthOptions>()
            .WithProperty(x => x.TwitchAuthEndpoint, AuthEndpoint)
            .WithProperty(x => x.ClientId, clientId);
        var iOptions = Microsoft.Extensions.Options.Options.Create<TwitchAuthOptions>(options);
        var serverTiming = Substitute.For<IServerTiming>();
        var timeProvider = Substitute.For<TimeProvider>();
        var memoryCache = Substitute.For<IMemoryCache>();
        var twitchAuthCacheEntry = new TwitchAuthCacheEntry(accessToken, long.MaxValue);
        memoryCache.TryGetValue(GetCacheKey(clientId), out Arg.Any<TwitchAuthCacheEntry?>())
            .Returns(x =>
            {
                x[1] = twitchAuthCacheEntry;
                return true;
            });

        var twitchAuthService =
            new TwitchAuthService(logger, httpClientFactory, iOptions, serverTiming, timeProvider, memoryCache);

        //WHEN

        var twitchAuthResult = await twitchAuthService.ObtainAccessToken();

        //THEN

        twitchAuthResult.Should().NotBeNull();
        twitchAuthResult.AccessToken.Should().BeEquivalentTo(accessToken);

        Received.InOrder(() =>
        {
            memoryCache.TryGetValue(GetCacheKey(clientId), out _);
            logger.LogDebug("Obtained twitch auth data from cache.");
        });

        memoryCache.DidNotReceive().CreateEntry(GetCacheKey(clientId));
        logger.DidNotReceive().LogInformation("Obtained twitch auth data from twitch auth service.");
    }

    [Test]
    public async Task ObtainAccessToken_Should_ThrowTwitchAuthExceptionIfResponseIsNull()
    {
        //GIVEN
        var logger = Substitute.For<ILogger<TwitchAuthService>>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        var mockedResponse = mockHttpMessageHandler
            .PrepareMockResponse(new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent(string.Empty) })
            .ForMethod(HttpMethod.Post);

        var client = mockHttpMessageHandler.ToHttpClient();
        client.BaseAddress = new Uri("https://id.twitch.tv");
        httpClientFactory.CreateClient("TwitchAuthHttpClient").Returns(client);
        var clientId = Any.String();
        var options = Any.Instance<TwitchAuthOptions>()
            .WithProperty(x => x.TwitchAuthEndpoint, AuthEndpoint)
            .WithProperty(x => x.ClientId, clientId);
        var iOptions = Microsoft.Extensions.Options.Options.Create<TwitchAuthOptions>(options);
        var serverTiming = Substitute.For<IServerTiming>();
        var timeProvider = Substitute.For<TimeProvider>();
        var memoryCache = Substitute.For<IMemoryCache>();

        var twitchAuthService =
            new TwitchAuthService(logger, httpClientFactory, iOptions, serverTiming, timeProvider, memoryCache);

        //WHEN

        await twitchAuthService.Awaiting(x => x.ObtainAccessToken()).Should().ThrowAsync<TwitchAuthException>();
        //THEN

        mockHttpMessageHandler.CountResponseReturns(mockedResponse).Should().Be(1);
        Received.InOrder(() =>
        {
            memoryCache.TryGetValue(GetCacheKey(clientId), out _);
            logger.LogWarning($"{nameof(TwitchAuthResponse)} is null.");
        });

        memoryCache.DidNotReceive().CreateEntry(GetCacheKey(clientId));
        logger.DidNotReceive().LogInformation("Obtained twitch auth data from twitch auth service.");
    }

    [Test]
    public async Task ObtainAccessToken_Should_ThrowTwitchAuthExceptionIfAccessTokenIsNull()
    {
        //GIVEN
        var logger = Substitute.For<ILogger<TwitchAuthService>>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        var accessToken = string.Empty;
        var mockedResponse = mockHttpMessageHandler
            .PrepareMockResponse(TwitchResponse(accessToken, (DateTime.UtcNow.AddMonths(2) - DateTime.UtcNow).Seconds))
            .ForMethod(HttpMethod.Post);

        var client = mockHttpMessageHandler.ToHttpClient();
        client.BaseAddress = new Uri("https://id.twitch.tv");
        httpClientFactory.CreateClient("TwitchAuthHttpClient").Returns(client);
        var clientId = Any.String();
        var options = Any.Instance<TwitchAuthOptions>()
            .WithProperty(x => x.TwitchAuthEndpoint, AuthEndpoint)
            .WithProperty(x => x.ClientId, clientId);
        var iOptions = Microsoft.Extensions.Options.Options.Create<TwitchAuthOptions>(options);
        var serverTiming = Substitute.For<IServerTiming>();
        var timeProvider = Substitute.For<TimeProvider>();
        var memoryCache = Substitute.For<IMemoryCache>();

        var twitchAuthService =
            new TwitchAuthService(logger, httpClientFactory, iOptions, serverTiming, timeProvider, memoryCache);

        //WHEN

        await twitchAuthService.Awaiting(x => x.ObtainAccessToken()).Should().ThrowAsync<TwitchAuthException>();
        //THEN

        mockHttpMessageHandler.CountResponseReturns(mockedResponse).Should().Be(1);
        Received.InOrder(() =>
        {
            memoryCache.TryGetValue(GetCacheKey(clientId), out _);
            logger.LogWarning($"{nameof(TwitchAuthResponse.AccessToken)} is null.");
        });

        memoryCache.DidNotReceive().CreateEntry(GetCacheKey(clientId));
        logger.DidNotReceive().LogInformation("Obtained twitch auth data from twitch auth service.");
    }

    [Test]
    public void GetClientId_Should_ReturnClientId()
    {
        //GIVEN
        var logger = Substitute.For<ILogger<TwitchAuthService>>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var client = Substitute.For<HttpClient>();
        httpClientFactory.CreateClient("TwitchAuthHttpClient").Returns(client);
        var clientId = Any.String();
        var options = Any.Instance<TwitchAuthOptions>()
            .WithProperty(x => x.TwitchAuthEndpoint, AuthEndpoint)
            .WithProperty(x => x.ClientId, clientId);
        var iOptions = Microsoft.Extensions.Options.Options.Create<TwitchAuthOptions>(options);
        var serverTiming = Substitute.For<IServerTiming>();
        var timeProvider = Substitute.For<TimeProvider>();
        var memoryCache = Substitute.For<IMemoryCache>();

        var twitchAuthService =
            new TwitchAuthService(logger, httpClientFactory, iOptions, serverTiming, timeProvider, memoryCache);

        //WHEN
        var obtainedClientId = twitchAuthService.GetClientId();

        //THEN
        obtainedClientId.Should().BeEquivalentTo(clientId);
    }

    private static string GetCacheKey(string clientId)
    {
        return $"{clientId}-twitchAuthResponse";
    }


    private static HttpResponseMessage TwitchResponse(string accessToken, long expiresIn)
    {
        var twitchAuthResponse = new TwitchAuthResponse()
        {
            AccessToken = accessToken,
            ExpiresIn = expiresIn,
            TokenType = "bearer"
        };
        var response = new HttpResponseMessage()
        {
            Content = new StringContent(JsonConvert.SerializeObject(twitchAuthResponse,
                JsonConvertSerializers.SnakeCaseNaming))
        };

        return response;
    }
}
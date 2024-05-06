using Lib.ServerTiming;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourGamesList.Common.Http;
using YourGamesList.Common.Options;
using YourGamesList.Common.Services.TwitchAuth.Exceptions;
using YourGamesList.Common.Services.TwitchAuth.Responses;

namespace YourGamesList.Common.Services.TwitchAuth;

public class TwitchAuthService : ITwitchAuthService
{
    private const string ServerTimingMetric = "twitchAuth";

    private readonly ILogger<TwitchAuthService> _logger;
    private readonly IServerTiming _serverTiming;
    private readonly TimeProvider _timeProvider;
    private readonly TwitchAuthOptions _options;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="httpClientFactory">Needs named client: "TwitchAuthHttpClient"</param>
    /// <param name="options"></param>
    /// <param name="serverTiming"></param>
    /// <param name="timeProvider"></param>
    /// <param name="memoryCache"></param>
    public TwitchAuthService(ILogger<TwitchAuthService> logger,  IHttpClientFactory httpClientFactory, IOptions<TwitchAuthOptions> options,
        IServerTiming serverTiming, TimeProvider timeProvider,  IMemoryCache memoryCache)
    {
        _logger = logger;
        _serverTiming = serverTiming;
        _timeProvider = timeProvider;
        _options = options.Value;
        _httpClient = httpClientFactory.CreateClient("TwitchAuthHttpClient");
        _memoryCache = memoryCache;
    }


    public async Task<TwitchAuthResult> ObtainAccessToken(CancellationToken token = default)
    {
        using var _ = _serverTiming.TimeAction(ServerTimingMetric);

        if (_memoryCache.TryGetValue<TwitchAuthCacheEntry>(GetCacheKey(_options.ClientId), out var cachedResponse)
            && cachedResponse != null
            && cachedResponse.ExpiryDate > GetCurrentTimestampInSeconds())
        {
            _logger.LogDebug("Obtained twitch auth data from cache.");
            return new TwitchAuthResult(cachedResponse.AccessToken);
        }

        var message = new HttpRequestMessageBuilder()
            .WithMethod(HttpMethod.Post)
            .WithUri(_options.TwitchAuthEndpoint, uriKind: UriKind.Relative)
            .WithFormUrlEncodedContent(CreateBodyForAuthRequest(_options.ClientId, _options.ClientSecret))
            .Build();

        var res = await _httpClient.SendAsync(message, token);
        var content = await res.Content.ReadAsStringAsync(token);
        var twitchAuthResponse =
            JsonConvert.DeserializeObject<TwitchAuthResponse>(content, JsonConvertSerializers.SnakeCaseNaming);

        if (twitchAuthResponse is null)
        {
            _logger.LogWarning($"{nameof(TwitchAuthResponse)} is null.");
            throw new TwitchAuthException();
        }

        if (string.IsNullOrEmpty(twitchAuthResponse.AccessToken))
        {
            _logger.LogWarning($"{nameof(TwitchAuthResponse.AccessToken)} is null.");

            throw new TwitchAuthException();
        }

        _logger.LogInformation("Obtained twitch auth data from twitch auth service.");

        SetCacheEntry(twitchAuthResponse);

        return new TwitchAuthResult(twitchAuthResponse.AccessToken);
    }

    private void SetCacheEntry(TwitchAuthResponse twitchAuthResponse)
    {
        var expirationEntryDate = GetCurrentTimestampInSeconds() + twitchAuthResponse.ExpiresIn -
                                  TimeSpan.FromHours(1).Seconds;
        var cacheEntry = new TwitchAuthCacheEntry(twitchAuthResponse.AccessToken, expirationEntryDate);
        var absoluteCacheExpiration = TimeSpan.FromSeconds(twitchAuthResponse.ExpiresIn) - TimeSpan.FromHours(1);

        _memoryCache.Set(GetCacheKey(_options.ClientId), cacheEntry, absoluteCacheExpiration);

        _logger.LogDebug("Saved twitch auth data in cache.");
    }

    private long GetCurrentTimestampInSeconds()
    {
        return _timeProvider.GetUtcNow().ToUnixTimeSeconds();
    }

    public string GetClientId()
    {
        return _options.ClientId;
    }

    private static string GetCacheKey(string clientId)
    {
        return $"{clientId}-twitchAuthResponse";
    }

    private static Dictionary<string, string> CreateBodyForAuthRequest(string clientId, string clientSecret)
    {
        var parameters = new Dictionary<string, string>()
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "grant_type", "client_credentials" },
        };
        return parameters;
    }
}
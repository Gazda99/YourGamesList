using System.Net;
using Igdb.Model.Helpers;
using Microsoft.Extensions.Logging;
using YourGamesList.Common.Http;
using YourGamesList.Common.Services.TwitchAuth;
using YourGamesList.IgdbScraper.Services.IgdbClient.Exceptions;

namespace YourGamesList.IgdbScraper.Services.IgdbClient;

public class IgdbClient : IIgdbClient
{
    private readonly ILogger<IgdbClient> _logger;
    private readonly HttpClient _httpClient;

    private readonly ITwitchAuthService _twitchAuthService;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="httpClientFactory">Needs named client: "IgdbHttpClient"</param>
    /// <param name="twitchAuthService"></param>
    public IgdbClient(ILogger<IgdbClient> logger, IHttpClientFactory httpClientFactory,
        ITwitchAuthService twitchAuthService)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("IgdbHttpClient");
        _twitchAuthService = twitchAuthService;
    }

    public async Task<IEnumerable<TResponse>> FetchData<TResponse, TEndpoint>(string requestBody)
    {
        var endpoint = IgdbEndpoints.GetEndpointBasedOnType<TEndpoint>();
        return await FetchDataInternal<TResponse>(requestBody, endpoint);
    }

    public async Task<IEnumerable<T>> FetchData<T>(string requestBody)
    {
        var endpoint = IgdbEndpoints.GetEndpointBasedOnType<T>();
        return await FetchDataInternal<T>(requestBody, endpoint);
    }

    public async Task<IEnumerable<T>> FetchData<T>(string requestBody, string endpoint)
    {
        return await FetchDataInternal<T>(requestBody, endpoint);
    }


    private async Task<IEnumerable<T>> FetchDataInternal<T>(string requestBody, string endpoint)
    {
        try
        {
            var bearerToken = await _twitchAuthService.ObtainAccessToken();
            var message = HttpRequestMessageBuilder.Create
                .WithMethod(HttpMethod.Post)
                .WithUri(endpoint, uriKind: UriKind.Relative)
                .WithHeaders(GetHeaderWithClientId(_twitchAuthService.GetClientId()))
                .WithBearerToken(bearerToken.ToString())
                .WithStringContent(requestBody)
                .Build();

            var res = await _httpClient.SendAsync(message);

            if (res.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogWarning(
                    $"Response from IGDB service is not successful, with status code: {(int) res.StatusCode}");
                var responseContent = await res.Content.ReadAsStringAsync();
                throw new IgdbClientException(IgdbClientExceptionReason.StatusCodeNotSuccess,
                    "Response from IGDB service is not successful")
                {
                    Endpoint = endpoint,
                    Request = requestBody,
                    ResponseContent = responseContent
                };
            }

            var content = await res.Content.ReadAsStringAsync();

            IEnumerable<T> igdbEntities;
            try
            {
                igdbEntities =
                    JsonConvert.DeserializeObject<IEnumerable<T>>(content, IgdbSerializers.IgdbResponseSerializer)!;
            }
            catch (Exception e)
            {
                //Not logging responseContent due to its possible size
                _logger.LogWarning(e, "Cannot parse response from the IGDB service.");
                throw new IgdbClientException(IgdbClientExceptionReason.ParsingResponse,
                    "Cannot parse response from the IGDB service.", e)
                {
                    Endpoint = endpoint,
                    Request = requestBody
                };
            }

            return igdbEntities;
        }
        catch (Exception e)
        {
            //Not logging responseContent due to its possible size
            _logger.LogWarning(e, "Unhandled exception occured when calling the IGDB service.");
            throw new IgdbClientException(IgdbClientExceptionReason.Other,
                "Unhandled exception occured when calling the IGDB service.", e)
            {
                Endpoint = endpoint,
                Request = requestBody
            };
        }
    }


    private static Dictionary<string, string> GetHeaderWithClientId(string clientId)
    {
        return new Dictionary<string, string>()
        {
            { "Client-ID", clientId }
        };
    }
}
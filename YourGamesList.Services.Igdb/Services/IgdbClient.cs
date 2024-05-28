using System.Net;
using Igdb.Model.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourGamesList.Common;
using YourGamesList.Common.Http;
using YourGamesList.Services.Igdb.Exceptions;
using YourGamesList.Services.Igdb.Internal;
using YourGamesList.Services.Igdb.Model;
using YourGamesList.Services.Igdb.Options;
using YourGamesList.Services.Twitch.Services;

namespace YourGamesList.Services.Igdb.Services;

public class IgdbClient : IIgdbClient
{
    private readonly ILogger<IgdbClient> _logger;
    private readonly HttpClient _httpClient;

    private readonly ITwitchAuthService _twitchAuthService;
    private readonly IgdbClientOptions _options;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="httpClientFactory">Needs named client: "IgdbHttpClient"</param>
    /// <param name="twitchAuthService"></param>
    public IgdbClient(ILogger<IgdbClient> logger, IHttpClientFactory httpClientFactory,
        ITwitchAuthService twitchAuthService, IOptions<IgdbClientOptions> options)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("IgdbHttpClient");
        _twitchAuthService = twitchAuthService;
        _options = options.Value;
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
            var twitchAccessToken = await _twitchAuthService.ObtainAccessToken();
            var message = HttpRequestMessageBuilder.Create
                .WithMethod(HttpMethod.Post)
                .WithUri(endpoint, uriKind: UriKind.Relative)
                .WithHeaders(GetHeaderWithClientId(_twitchAuthService.GetClientId()))
                .WithBearerToken(twitchAccessToken)
                .WithStringContent(requestBody)
                .Build();

            var res = await _httpClient.SendAsync(message);

            if (res.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogWarning(
                    $"Response from IGDB service is not successful, with status code: {(int) res.StatusCode}");
                var responseContent = await res.Content.ReadAsStringAsync();
                throw new IgdbClientFetchDataException(IgdbClientExceptionReason.StatusCodeNotSuccess,
                    IgdbClientExceptionMessages.ResponseUnsuccessful)
                {
                    StatusCode = (int) res.StatusCode,
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
                //Not logging responseContent due to its possible huge size
                _logger.LogWarning(e, IgdbClientExceptionMessages.ParsingResponseProblem);
                throw new IgdbClientFetchDataException(IgdbClientExceptionReason.ParsingResponse,
                    IgdbClientExceptionMessages.ParsingResponseProblem, e)
                {
                    Endpoint = endpoint,
                    Request = requestBody
                };
            }

            return igdbEntities;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, IgdbClientExceptionMessages.UnhandledException);
            throw new IgdbClientFetchDataException(IgdbClientExceptionReason.Other,
                IgdbClientExceptionMessages.UnhandledException, ex)
            {
                Endpoint = endpoint,
                Request = requestBody
            };
        }
    }

    public async Task<IEnumerable<IgdbWebhook>> ListWebhooks()
    {
        try
        {
            _logger.LogInformation("Trying to obtain information about registered webhooks.");

            var twitchAccessToken = await _twitchAuthService.ObtainAccessToken();
            var message = HttpRequestMessageBuilder.Create
                .WithMethod(HttpMethod.Get)
                .WithUri("webhooks", uriKind: UriKind.Relative)
                .WithHeaders(GetHeaderWithClientId(_twitchAuthService.GetClientId()))
                .WithBearerToken(twitchAccessToken)
                .Build();

            var res = await _httpClient.SendAsync(message);

            if (res.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("Successfully got information about registered webhooks.");

                var webhooks = await ParseWebhookResponseContent(res.Content);

                return webhooks;
            }
            else
            {
                var content = await res.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    $"Error obtaining list of webhooks. Status code: {res.StatusCode}. Content: {content}");

                throw new IgdbClientWebhookException(IgdbClientExceptionReason.StatusCodeNotSuccess,
                    IgdbClientExceptionMessages.ResponseUnsuccessful)
                {
                    StatusCode = (int) res.StatusCode,
                    Operation = "List",
                    ResponseContent = content,
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, IgdbClientExceptionMessages.UnhandledException);
            throw new IgdbClientWebhookException(IgdbClientExceptionReason.Other,
                IgdbClientExceptionMessages.UnhandledException, ex)
            {
                Operation = "List",
            };
        }
    }

    public async Task DeleteWebhook(long webhookId)
    {
        try
        {
            _logger.LogInformation($"Trying to remove webhook {webhookId}");

            var twitchAccessToken = await _twitchAuthService.ObtainAccessToken();
            var message = HttpRequestMessageBuilder.Create
                .WithMethod(HttpMethod.Delete)
                .WithUri($"webhooks/{webhookId}", uriKind: UriKind.Relative)
                .WithHeaders(GetHeaderWithClientId(_twitchAuthService.GetClientId()))
                .WithBearerToken(twitchAccessToken)
                .Build();

            var res = await _httpClient.SendAsync(message);

            if (res.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation($"Successfully removed webhook {webhookId}.");
            }
            else
            {
                var content = await res.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    $"Error when deleting webhook {webhookId}. Status code: {res.StatusCode}. Content: {content}");

                throw new IgdbClientWebhookException(IgdbClientExceptionReason.StatusCodeNotSuccess,
                    IgdbClientExceptionMessages.ResponseUnsuccessful)
                {
                    StatusCode = (int) res.StatusCode,
                    Operation = "Delete",
                    ResponseContent = content,
                    WebhookId = webhookId
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, IgdbClientExceptionMessages.UnhandledException);
            throw new IgdbClientWebhookException(IgdbClientExceptionReason.Other,
                IgdbClientExceptionMessages.UnhandledException, ex)
            {
                Operation = "Delete",
                WebhookId = webhookId
            };
        }
    }

    public async Task<long> RegisterCreateWebhook(string endpoint)
    {
        return await RegisterWebhook(WebhookCreateMethod.Create, endpoint);
    }

    public async Task<long> RegisterDeleteWebhook(string endpoint)
    {
        return await RegisterWebhook(WebhookCreateMethod.Delete, endpoint);
    }

    public async Task<long> RegisterUpdateWebhook(string endpoint)
    {
        return await RegisterWebhook(WebhookCreateMethod.Update, endpoint);
    }

    private async Task<long> RegisterWebhook(string method, string endpoint)
    {
        try
        {
            _logger.LogInformation($"Trying to register {method} webhook for {endpoint} endpoint.");
            //TODO: think bout url
            var url = "";

            var twitchAccessToken = await _twitchAuthService.ObtainAccessToken();
            var message = HttpRequestMessageBuilder.Create
                .WithMethod(HttpMethod.Post)
                .WithUri($"{endpoint}/webhooks", uriKind: UriKind.Relative)
                .WithHeaders(GetHeaderWithClientId(_twitchAuthService.GetClientId()))
                .WithBearerToken(twitchAccessToken)
                .WithFormUrlEncodedContent(PrepareBodyForWebhookRequest(url, method, _options.WebhookSecret))
                .Build();

            var res = await _httpClient.SendAsync(message);

            if (res.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation($"Successfully registered {method} webhook for {endpoint} endpoint.");

                var webhooks = await ParseWebhookResponseContent(res.Content);
                return webhooks.First().Id;
            }
            else
            {
                var content = await res.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    $"Error when registering {method} webhook for {endpoint} endpoint. Status code: {res.StatusCode}. Content: {content}");
                throw new IgdbClientWebhookException(IgdbClientExceptionReason.StatusCodeNotSuccess,
                    IgdbClientExceptionMessages.ResponseUnsuccessful)
                {
                    StatusCode = (int) res.StatusCode,
                    Operation = "Register",
                    Method = method,
                    ResponseContent = content,
                    Endpoint = endpoint
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, IgdbClientExceptionMessages.UnhandledException);
            throw new IgdbClientWebhookException(IgdbClientExceptionReason.Other,
                IgdbClientExceptionMessages.UnhandledException, ex)
            {
                Operation = "Register",
                Method = method,
                Endpoint = endpoint
            };
        }
    }

    private async Task<IEnumerable<IgdbWebhook>> ParseWebhookResponseContent(HttpContent content)
    {
        var stringContent = await content.ReadAsStringAsync();
        try
        {
            var webhooks =
                JsonConvert.DeserializeObject<IEnumerable<IgdbWebhook>>(stringContent,
                    JsonConvertSerializers.SnakeCaseNaming);
            return webhooks!;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, IgdbClientExceptionMessages.ParsingResponseProblem);
            throw new IgdbClientException(IgdbClientExceptionReason.ParsingResponse,
                IgdbClientExceptionMessages.ParsingResponseProblem, ex)
            {
                ResponseContent = stringContent,
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

    private static Dictionary<string, string> PrepareBodyForWebhookRequest(string url, string method, string secret)
    {
        return new Dictionary<string, string>()
        {
            { "url", url },
            { "method", method },
            { "secret", secret },
        };
    }
}
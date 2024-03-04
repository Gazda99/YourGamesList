using System.Net;
using Igdb.Model.Custom;
using Igdb.Model.Helpers;
using Microsoft.Extensions.Logging;
using YourGamesList.Common.Http;

namespace YourGamesList.IgdbScraper.Services;

public class MaxIdChecker : IMaxIdChecker
{
    private const string MaxIdQueryBody = "fields id; sort id desc; limit 1;";

    private readonly ILogger<MaxIdChecker> _logger;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="httpClientFactory">Needs named client: "IgdbHttpClient"</param>
    public MaxIdChecker(ILogger<MaxIdChecker> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("IgdbHttpClient");
    }

    public async Task<long> GetMaxId<T>(string clientId, string bearerToken)
    {
        var endpoint = IgdbEndpoints.GetEndpointBasedOnType<T>();
        return await GetMaxId(clientId, bearerToken, endpoint);
    }

    public async Task<long> GetMaxId(string clientId, string bearerToken, string endpoint)
    {
        _logger.LogInformation($"Trying to find max id using endpoint: {endpoint}");

        var message = HttpRequestMessageBuilder.Create
            .WithMethod(HttpMethod.Post)
            .WithUri(endpoint, uriKind: UriKind.Relative)
            .WithHeaders(GetHeaderWithClientId(clientId))
            .WithBearerToken(bearerToken)
            .WithStringContent(MaxIdQueryBody)
            .Build();
        var res = await _httpClient.SendAsync(message);

        if (res.StatusCode != HttpStatusCode.OK)
        {
            _logger.LogError(
                $"Could not obtain max id using endpoint: {endpoint}. Reason: StatusCode is not OK. StatusCode: {res.StatusCode}");
            return -1;
        }

        var content = await res.Content.ReadAsStringAsync();
        var returnedObject = JsonConvert.DeserializeObject<IEnumerable<IdAndName>>(content) ?? Array.Empty<IdAndName>();

        if (!returnedObject.Any())
        {
            _logger.LogError($"Could not obtain max id using endpoint: {endpoint}. Reason: list is empty.");
            return -1;
        }

        var first = returnedObject.FirstOrDefault();
        if (first == null)
        {
            _logger.LogError($"Could not obtain max id using endpoint: {endpoint}. Reason: first item is null.");
            return -1;
        }

        if (first.Id == null)
        {
            _logger.LogError($"Could not obtain max id using endpoint: {endpoint}. Reason: first item.Id is null.");
            return -1;
        }

        _logger.LogInformation($"For endpoint {endpoint}, found max id: {first.Id}");
        return first.Id.Value;
    }

    public async Task<long> GetCount<T>(string clientId, string bearerToken)
    {
        var endpoint = IgdbEndpoints.GetEndpointBasedOnType<T>();
        return await GetCount(clientId, bearerToken, endpoint);
    }

    public async Task<long> GetCount(string clientId, string bearerToken, string endpoint)
    {
        _logger.LogInformation($"Trying to find count using endpoint: {endpoint}");

        var message = HttpRequestMessageBuilder.Create
            .WithMethod(HttpMethod.Post)
            .WithUri(IgdbEndpoints.MultiQuery, uriKind: UriKind.Relative)
            .WithHeaders(GetHeaderWithClientId(clientId))
            .WithBearerToken(bearerToken)
            .WithStringContent(CountQueryBody(endpoint))
            .Build();
        var res = await _httpClient.SendAsync(message);

        if (res.StatusCode != HttpStatusCode.OK)
        {
            _logger.LogError(
                $"Could not obtain count using endpoint: {endpoint}. Reason: StatusCode is not OK. StatusCode: {res.StatusCode}");
            return -1;
        }

        var content = await res.Content.ReadAsStringAsync();
        var returnedObject = JsonConvert.DeserializeObject<IEnumerable<MultiQueryCount>>(content) ??
                             Array.Empty<MultiQueryCount>();

        if (!returnedObject.Any())
        {
            _logger.LogError($"Could not obtain count using endpoint: {endpoint}. Reason: list is empty.");
            return -1;
        }

        var first = returnedObject.FirstOrDefault();
        if (first == null)
        {
            _logger.LogError($"Could not obtain count using endpoint: {endpoint}. Reason: first item is null.");
            return -1;
        }

        _logger.LogInformation($"For endpoint {endpoint}, found count: {first.Count}");
        return first.Count;
    }

    private static string CountQueryBody(string endpoint)
    {
        return $"query {endpoint}/count \"Count of {endpoint}\" {{}};";
    }

    private static Dictionary<string, string> GetHeaderWithClientId(string clientId)
    {
        return new Dictionary<string, string>()
        {
            { "Client-ID", clientId }
        };
    }
}
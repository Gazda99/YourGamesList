using Igdb.Model.Custom;
using Igdb.Model.Helpers;
using Microsoft.Extensions.Logging;
using YourGamesList.IgdbScraper.Services.IgdbClient;
using YourGamesList.IgdbScraper.Services.IgdbClient.Exceptions;

namespace YourGamesList.IgdbScraper.Services.MaxIdChecker;

public class MaxIdChecker : IMaxIdChecker
{
    private const string MaxIdQueryBody = "fields id; sort id desc; limit 1;";

    private readonly ILogger<MaxIdChecker> _logger;
    private readonly IIgdbClient _igdbClient;

    public MaxIdChecker(ILogger<MaxIdChecker> logger, IIgdbClient igdbClient)
    {
        _logger = logger;
        _igdbClient = igdbClient;
    }

    public async Task<long> GetMaxId<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = IgdbEndpoints.GetEndpointBasedOnType<T>();
        return await GetMaxId(endpoint, cancellationToken);
    }

    // TODO: change return -1 to throw exception
    public async Task<long> GetMaxId(string endpoint, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Trying to find max id using endpoint: {endpoint}");

        IEnumerable<IdAndName> res;
        try
        {
            res = await _igdbClient.FetchData<IdAndName>(MaxIdQueryBody, endpoint);
        }
        catch (IgdbClientException ex)
        {
            _logger.LogError(ex, $"{CouldNotObtainMaxIdLog(endpoint)} Reason: {ex.Reason}.");
            return -1;
        }

        if (!res.Any())
        {
            _logger.LogError($"{CouldNotObtainMaxIdLog(endpoint)} Reason: list is empty.");
            return -1;
        }

        var first = res.First();

        if (first.Id == null)
        {
            _logger.LogError($"{CouldNotObtainMaxIdLog(endpoint)} Reason: first item.Id is null.");
            return -1;
        }

        _logger.LogInformation($"For endpoint {endpoint}, found max id: {first.Id}");
        return first.Id.Value;
    }

    public async Task<long> GetCount<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = IgdbEndpoints.GetEndpointBasedOnType<T>();
        return await GetCount(endpoint, cancellationToken);
    }

    public async Task<long> GetCount(string endpoint, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Trying to find count using endpoint: {endpoint}");

        IEnumerable<MultiQueryCount> res;
        try
        {
            res = await _igdbClient.FetchData<MultiQueryCount>(CountQueryBody(endpoint), IgdbEndpoints.MultiQuery);
        }
        catch (IgdbClientException ex)
        {
            _logger.LogError(ex, $"{CouldNotObtainCountLog(endpoint)} Reason: {ex.Reason}.");
            return -1;
        }

        if (!res.Any())
        {
            _logger.LogError($"{CouldNotObtainCountLog(endpoint)} Reason: list is empty.");
            return -1;
        }

        var first = res.First();

        _logger.LogInformation($"For endpoint {endpoint}, found count: {first.Count}");
        return first.Count;
    }

    private static string CountQueryBody(string endpoint)
    {
        return $"query {endpoint}/count \"Count of {endpoint}\" {{}};";
    }

    private static string CouldNotObtainMaxIdLog(string endpoint)
    {
        return $"Could not obtain max id using endpoint: {endpoint}.";
    }

    private static string CouldNotObtainCountLog(string endpoint)
    {
        return $"Could not obtain count using endpoint: {endpoint}.";
    }
}
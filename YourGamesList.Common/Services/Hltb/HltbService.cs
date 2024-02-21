using Microsoft.Extensions.Logging;
using YourGamesList.Common.Http;
using YourGamesList.Common.Services.Hltb.Requests;
using YourGamesList.Common.Services.Hltb.Responses;

namespace YourGamesList.Common.Services.Hltb;

public class HltbService : IHltbService
{
    private readonly ILogger<HltbService> _logger;
    private readonly HttpClient _httpClient;


    public HltbService(ILogger<HltbService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<HltbSearchResponse?> GetHowLongToBeatDataForGame(string gameName)
    {
        _logger.LogInformation($"Getting HowLongToBeat data for game: {gameName}");
        var request = PrepareSearchRequestMessage(gameName);
        var res = await _httpClient.SendAsync(request);
        var content = await res.Content.ReadAsStringAsync();
        
        try
        {
            var parsedResponse =
                JsonConvert.DeserializeObject<HltbSearchResponse>(content, JsonConvertSerializers.CamelCase);
            return parsedResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Could not parse HowLongToBeatResponse. Response: {content}");
            return null;
        }
    }

    private HttpRequestMessage PrepareSearchRequestMessage(string gameName)
    {
        var message = new HttpRequestMessageBuilder()
            .WithMethod(HttpMethod.Post)
            .WithUri("search", uriKind: UriKind.Relative)
            .WithHeader("Referer", _httpClient.BaseAddress?.ToString() ?? string.Empty)
            .WithHeader(HeaderDefs.YglUserAgentHeaderTuple)
            .WithStringContent(PrepareRequestBodyForSearch(gameName), ContentTypes.ApplicationJson)
            .Build();

        return message;
    }

    private static string PrepareRequestBodyForSearch(string gameName)
    {
        var gameSplit = gameName.Split(" ", StringSplitOptions.RemoveEmptyEntries);

        var requestData = new HltbSearchRequest()
        {
            SearchTerms = gameSplit
        };

        return JsonConvert.SerializeObject(requestData, JsonConvertSerializers.CamelCase);
    }
}
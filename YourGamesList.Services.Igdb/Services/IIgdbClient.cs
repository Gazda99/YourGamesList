using YourGamesList.Services.Igdb.Model;

namespace YourGamesList.Services.Igdb.Services;

public interface IIgdbClient
{
    //fetch data from endpoints
    Task<IEnumerable<T>> FetchData<T>(string requestBody);
    Task<IEnumerable<T>> FetchData<T>(string requestBody, string endpoint);
    Task<IEnumerable<TResponse>> FetchData<TResponse, TEndpoint>(string requestBody);

    //webhooks
    Task<IEnumerable<IgdbWebhook>> ListWebhooks();
    Task DeleteWebhook(long webhookId);
    Task<long> RegisterCreateWebhook(string endpoint);
    Task<long> RegisterDeleteWebhook(string endpoint);
    Task<long> RegisterUpdateWebhook(string endpoint);
}
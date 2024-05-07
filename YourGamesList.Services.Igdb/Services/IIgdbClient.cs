namespace YourGamesList.Services.Igdb.Services;

public interface IIgdbClient
{
    Task<IEnumerable<T>> FetchData<T>(string requestBody);
    Task<IEnumerable<T>> FetchData<T>(string requestBody, string endpoint);
    Task<IEnumerable<TResponse>> FetchData<TResponse, TEndpoint>(string requestBody);
}
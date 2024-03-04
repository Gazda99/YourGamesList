namespace YourGamesList.IgdbScraper.Services;

public interface IMaxIdChecker
{
    Task<long> GetMaxId<T>(string clientId, string bearerToken);
    Task<long> GetMaxId(string clientId, string bearerToken, string endpoint);
    Task<long> GetCount<T>(string clientId, string bearerToken);
    Task<long> GetCount(string clientId, string bearerToken, string endpoint);
}
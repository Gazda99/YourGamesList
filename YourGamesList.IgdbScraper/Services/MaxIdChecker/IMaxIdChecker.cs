namespace YourGamesList.IgdbScraper.Services.MaxIdChecker;

public interface IMaxIdChecker
{
    Task<long> GetMaxId<T>(CancellationToken cancellationToken = default);
    Task<long> GetMaxId(string endpoint, CancellationToken cancellationToken = default);
    Task<long> GetCount<T>(CancellationToken cancellationToken = default);
    Task<long> GetCount(string endpoint, CancellationToken cancellationToken = default);
}
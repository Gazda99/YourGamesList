namespace YourGamesList.Services.Igdb.Services;

public interface IScraper
{
    Task<IEnumerable<T>> Scrape<T>(CancellationToken cancellationToken = default);
}
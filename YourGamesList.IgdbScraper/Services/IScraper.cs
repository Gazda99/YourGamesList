namespace YourGamesList.IgdbScraper.Services;

public interface IScraper
{
    Task<IEnumerable<T>> Scrape<T>(CancellationToken cancellationToken = default);
}
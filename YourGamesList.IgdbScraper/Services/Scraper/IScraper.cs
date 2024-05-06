namespace YourGamesList.IgdbScraper.Services.Scraper;

public interface IScraper
{
    Task<IEnumerable<T>> Scrape<T>(CancellationToken cancellationToken = default);
}
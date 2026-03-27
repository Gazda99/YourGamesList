using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.Scraper.Model;
using YourGamesList.Common.Caching;

namespace YourGamesList.Api.Services.Scraper;

public interface IScraperCache
{
    bool TryGetCacheEntry([NotNullWhen(true)] out ScrapeInfoCacheEntry? cacheEntry);
    void WriteCache(ScrapeInfoCacheEntry entry);
    void Remove();
}

public class ScraperCache : IScraperCache
{
    private const string ScrapeStatusCacheKey = "igdb-scraper-status";

    private readonly ILogger<ScraperCache> _logger;
    private readonly ICacheProvider _cacheProvider;

    public ScraperCache(ILogger<ScraperCache> logger, [FromKeyedServices(CacheProviders.InMemory)] ICacheProvider cacheProvider)
    {
        _logger = logger;
        _cacheProvider = cacheProvider;
    }

    public bool TryGetCacheEntry([NotNullWhen(true)] out ScrapeInfoCacheEntry? cacheEntry)
    {
        if (!_cacheProvider.TryGet<ScrapeInfoCacheEntry>(ScrapeStatusCacheKey, out cacheEntry) || cacheEntry == null)
        {
            return false;
        }

        return true;
    }

    public void WriteCache(ScrapeInfoCacheEntry entry)
    {
        _cacheProvider.Set(ScrapeStatusCacheKey, entry);
    }

    public void Remove()
    {
        _cacheProvider.Remove(ScrapeStatusCacheKey);
    }
}
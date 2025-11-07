using AutoFixture;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Services.Scraper;
using YourGamesList.Api.Services.Scraper.Model;
using YourGamesList.Common.Caching;

namespace YourGamesList.Api.UnitTests.Services.Scraper;

public class ScraperCacheTests
{
    private const string ScrapeStatusCacheKey = "igdb-scraper-status";

    private IFixture _fixture;
    private ILogger<ScraperCache> _logger;
    private ICacheProvider _cacheProvider;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<ScraperCache>>();
        _cacheProvider = Substitute.For<ICacheProvider>();
    }

    [Test]
    public void TryGetCacheEntry_WhenCacheFound_ReturnsCacheEntryAndTrue()
    {
        //ARRANGE
        var expectedCacheEntry = _fixture.Create<ScrapeInfoCacheEntry>();
        _cacheProvider.TryGet<ScrapeInfoCacheEntry>(ScrapeStatusCacheKey, out Arg.Any<ScrapeInfoCacheEntry?>())
            .Returns(x =>
            {
                x[1] = expectedCacheEntry;
                return true;
            });

        var scraperCache = new ScraperCache(_logger, _cacheProvider);

        //ACT
        var res = scraperCache.TryGetCacheEntry(out var cacheEntry);

        //ASSERT
        Assert.That(res, Is.True);
        Assert.That(cacheEntry, Is.SameAs(expectedCacheEntry));

        _cacheProvider.Received(1).TryGet<ScrapeInfoCacheEntry>(ScrapeStatusCacheKey, out Arg.Any<ScrapeInfoCacheEntry?>());
    }

    [Test]
    public void TryGetCacheEntry_WhenCacheNotFound_ReturnsFalse()
    {
        //ARRANGE
        _cacheProvider.TryGet<ScrapeInfoCacheEntry>(ScrapeStatusCacheKey, out Arg.Any<ScrapeInfoCacheEntry?>())
            .Returns(x =>
            {
                x[1] = null;
                return false;
            });

        var scraperCache = new ScraperCache(_logger, _cacheProvider);

        //ACT
        var res = scraperCache.TryGetCacheEntry(out var cacheEntry);

        //ASSERT
        Assert.That(res, Is.False);
        Assert.That(cacheEntry, Is.Null);

        _cacheProvider.Received(1).TryGet<ScrapeInfoCacheEntry>(ScrapeStatusCacheKey, out Arg.Any<ScrapeInfoCacheEntry?>());
    }

    [Test]
    public void WriteCache_WritesCacheEntry()
    {
        //ARRANGE
        var cacheEntry = _fixture.Create<ScrapeInfoCacheEntry>();

        var scraperCache = new ScraperCache(_logger, _cacheProvider);

        //ACT
        scraperCache.WriteCache(cacheEntry);

        //ASSERT
        _cacheProvider.Received(1).Set(ScrapeStatusCacheKey, cacheEntry);
    }

    [Test]
    public void Remove_RemovesCacheEntry()
    {
        //ARRANGE
        var scraperCache = new ScraperCache(_logger, _cacheProvider);

        //ACT
        scraperCache.Remove();

        //ASSERT
        _cacheProvider.Received(1).Remove(ScrapeStatusCacheKey);
    }
}
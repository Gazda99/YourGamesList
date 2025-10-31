using System;

namespace YourGamesList.Api.Services.Scraper.Model;

public class ScrapeInfoCacheEntry
{
    public required string Id { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public bool IsCancellationRequested { get; set; } = false;
    public ScrapeStatus ScrapeStatus { get; set; }
}
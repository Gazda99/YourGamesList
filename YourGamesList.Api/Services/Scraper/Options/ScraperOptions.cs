using FluentValidation;

namespace YourGamesList.Api.Services.Scraper.Options;

public class ScraperOptions
{
    public const string SectionName = "Scraper";

    public int BatchSize { get; set; } = 500;
    public int ConcurrencyLevel { get; set; } = 4;
    public int RateLimitTimeFrameInMilliseconds { get; set; } = 1000;
    public int MaxConcurrentCallsWithinTimeFrame { get; set; } = 4;
}

internal sealed class ScraperOptionsValidator : AbstractValidator<ScraperOptions>
{
    public ScraperOptionsValidator()
    {
        RuleFor(x => x.BatchSize).GreaterThan(0);
        RuleFor(x => x.ConcurrencyLevel).GreaterThan(0);
        RuleFor(x => x.RateLimitTimeFrameInMilliseconds).GreaterThan(0);
        RuleFor(x => x.MaxConcurrentCallsWithinTimeFrame).GreaterThan(0);
    }
}
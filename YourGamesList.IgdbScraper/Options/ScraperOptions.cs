using FluentValidation;

namespace YourGamesList.IgdbScraper.Options;

public class ScraperOptions
{
    public const string OptionsName = nameof(ScraperOptions);

    public int BathSize { get; init; }
    public int RpsLimit { get; init; }
    public int DelayBetweenRequestsInMilliseconds { get; init; }
}

public class ScraperOptionsValidator : AbstractValidator<ScraperOptions>
{
    public ScraperOptionsValidator()
    {
        RuleFor(x => x.BathSize)
            .Must(x => x > 0)
            .NotEmpty();

        RuleFor(x => x.RpsLimit)
            .Must(x => x > 0)
            .NotEmpty();

        RuleFor(x => x.DelayBetweenRequestsInMilliseconds)
            .Must(x => x >= 0)
            .NotEmpty();
    }
}
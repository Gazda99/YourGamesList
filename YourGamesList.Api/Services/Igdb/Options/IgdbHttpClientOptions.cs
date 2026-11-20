using FluentValidation;
using YourGamesList.Common.Http;
using YourGamesList.Common.Options.Validators;

namespace YourGamesList.Api.Services.Igdb.Options;

public class IgdbHttpClientOptions
{
    public const string SectionName = "IgdbHttpClient";

    public required string BaseAddress { get; init; }
    /// <summary>
    /// Optional; when not configured, resilience defaults are used (see <see cref="HttpResilienceOptions"/>).
    /// </summary>
    public HttpResilienceOptions? Resilience { get; init; }
}

public class IgdbHttpClientOptionsValidator : AbstractValidator<IgdbHttpClientOptions>
{
    public IgdbHttpClientOptionsValidator()
    {
        RuleFor(x => x.BaseAddress).IsValidAbsoluteUrl();
        RuleFor(x => x.Resilience!)
            .SetValidator(new HttpResilienceOptionsValidator())
            .When(x => x.Resilience is not null);
    }
}
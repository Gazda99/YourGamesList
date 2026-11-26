using FluentValidation;
using YourGamesList.Common.Http;
using YourGamesList.Common.Options.Validators;

namespace YourGamesList.Api.Services.Twitch.Options;

public class TwitchAuthHttpClientOptions
{
    public const string SectionName = "TwitchAuthHttpClient";

    public required string BaseAddress { get; init; }
    /// <summary>
    /// Optional; when not configured, resilience defaults are used (see <see cref="HttpResilienceOptions"/>).
    /// </summary>
    public HttpResilienceOptions? Resilience { get; init; }
}

public class TwitchAuthHttpClientOptionsValidator : AbstractValidator<TwitchAuthHttpClientOptions>
{
    public TwitchAuthHttpClientOptionsValidator()
    {
        RuleFor(x => x.BaseAddress).IsValidAbsoluteUrl();
        RuleFor(x => x.Resilience!)
            .SetValidator(new HttpResilienceOptionsValidator())
            .When(x => x.Resilience is not null);
    }
}
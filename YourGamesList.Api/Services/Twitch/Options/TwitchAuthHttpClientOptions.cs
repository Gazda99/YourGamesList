using FluentValidation;
using YourGamesList.Common.Options.Validators;

namespace YourGamesList.Api.Services.Twitch.Options;

public class TwitchAuthHttpClientOptions
{
    public const string SectionName = "TwitchAuthHttpClient";

    public required string BaseAddress { get; init; }
}

internal sealed class TwitchAuthHttpClientOptionsValidator : AbstractValidator<TwitchAuthHttpClientOptions>
{
    public TwitchAuthHttpClientOptionsValidator()
    {
        RuleFor(x => x.BaseAddress).IsValidAbsoluteUrl();
    }
}
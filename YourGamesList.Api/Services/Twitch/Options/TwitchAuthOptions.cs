using FluentValidation;

namespace YourGamesList.Api.Services.Twitch.Options;

public class TwitchAuthOptions
{
    public const string SectionName = "TwitchAuth";

    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
}

internal sealed class TwitchAuthOptionsValidator : AbstractValidator<TwitchAuthOptions>
{
    public TwitchAuthOptionsValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.ClientSecret).NotEmpty();
    }
}
using FluentValidation;

namespace YourGamesList.Services.Twitch.Options;

public class TwitchAuthOptions
{
    public const string OptionsName = nameof(TwitchAuthOptions);

    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string TwitchAuthEndpoint { get; init; } = string.Empty;
}

public class TwitchAuthOptionsValidator : AbstractValidator<TwitchAuthOptions>
{
    public TwitchAuthOptionsValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty();

        RuleFor(x => x.ClientSecret)
            .NotEmpty();

        RuleFor(x => x.TwitchAuthEndpoint)
            .NotEmpty();
    }
}
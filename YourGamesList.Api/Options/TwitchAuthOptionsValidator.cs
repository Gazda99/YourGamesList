using FluentValidation;
using YourGamesList.Common.Options;

namespace YourGamesList.Api.Options;

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
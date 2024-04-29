using FluentValidation;
using YourGamesList.Common.Options;
using YourGamesList.Common.Options.Validators;

namespace YourGamesList.Api.Options;

public class HltbHttpClientOptions
{
    public const string OptionsName = nameof(HltbHttpClientOptions);

    public string BaseAddress { get; init; } = string.Empty;
}

public class HltbHttpClientOptionsValidator : AbstractValidator<HltbHttpClientOptions>
{
    public HltbHttpClientOptionsValidator()
    {
        RuleFor(x => x.BaseAddress)
            .NotEmpty();
        RuleFor(x => x.BaseAddress)
            .Must(x => OptionsValidatorRules.IsValidUrl(x))
            .WithMessage(x =>
                $"{nameof(TwitchAuthHttpClientOptions.BaseAddress)} \"{x.BaseAddress}\" is not a valid URL");
    }
}
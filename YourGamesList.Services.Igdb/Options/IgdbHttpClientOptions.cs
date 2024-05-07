using FluentValidation;
using YourGamesList.Common.Options.Validators;

namespace YourGamesList.Services.Igdb.Options;

public class IgdbHttpClientOptions
{
    public const string OptionsName = nameof(IgdbHttpClientOptions);

    public string BaseAddress { get; init; } = string.Empty;
}

public class IgdbHttpClientOptionsValidator : AbstractValidator<IgdbHttpClientOptions>
{
    public IgdbHttpClientOptionsValidator()
    {
        RuleFor(x => x.BaseAddress)
            .NotEmpty();
        RuleFor(x => x.BaseAddress)
            .Must(x => OptionsValidatorRules.IsValidUrl(x))
            .WithMessage(x =>
                $"{nameof(IgdbHttpClientOptions.BaseAddress)} \"{x.BaseAddress}\" is not a valid URL");
    }
}
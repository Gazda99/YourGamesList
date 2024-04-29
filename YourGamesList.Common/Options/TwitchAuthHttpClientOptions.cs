﻿using FluentValidation;
using YourGamesList.Common.Options.Validators;

namespace YourGamesList.Common.Options;

public class TwitchAuthHttpClientOptions
{
    public const string OptionsName = nameof(TwitchAuthHttpClientOptions);

    public string BaseAddress { get; init; } = string.Empty;
}

public class TwitchAuthHttpClientOptionsValidator : AbstractValidator<TwitchAuthHttpClientOptions>
{
    public TwitchAuthHttpClientOptionsValidator()
    {
        RuleFor(x => x.BaseAddress)
            .NotEmpty();
        RuleFor(x => x.BaseAddress)
            .Must(x => OptionsValidatorRules.IsValidUrl(x))
            .WithMessage(x =>
                $"{nameof(TwitchAuthHttpClientOptions.BaseAddress)} \"{x.BaseAddress}\" is not a valid URL");
    }
}
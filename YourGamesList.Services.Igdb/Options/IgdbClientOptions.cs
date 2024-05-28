using FluentValidation;

namespace YourGamesList.Services.Igdb.Options;

public class IgdbClientOptions
{
    public const string OptionsName = nameof(IgdbClientOptions);

    public string WebhookSecret { get; set; } = string.Empty;
}

public class IgdbClientOptionsValidator : AbstractValidator<IgdbClientOptions>
{
    public IgdbClientOptionsValidator()
    {
        RuleFor(x => x.WebhookSecret)
            .NotEmpty();
    }
}
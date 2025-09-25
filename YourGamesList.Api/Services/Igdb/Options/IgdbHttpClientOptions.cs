using FluentValidation;
using YourGamesList.Common.Options.Validators;

namespace YourGamesList.Api.Services.Igdb.Options;

public class IgdbHttpClientOptions
{
    public const string SectionName = "IgdbHttpClient";

    public required string BaseAddress { get; set; }
}

internal sealed class IgdbHttpClientOptionsValidator : AbstractValidator<IgdbHttpClientOptions>
{
    public IgdbHttpClientOptionsValidator()
    {
        RuleFor(x => x.BaseAddress).IsValidAbsoluteUrl();
    }
}
using FluentValidation;
using YourGamesList.Common.Options.Validators;

namespace YourGamesList.Web.Page.Services.Ygl.Options;

public class YglApiHttpClientOptions
{
    public const string SectionName = "YglApiHttpClient";

    public required string BaseAddress { get; init; }
}

public sealed class YglApiHttpClientOptionsValidator : AbstractValidator<YglApiHttpClientOptions>
{
    public YglApiHttpClientOptionsValidator()
    {
        RuleFor(x => x.BaseAddress).IsValidAbsoluteUrl();
    }
}
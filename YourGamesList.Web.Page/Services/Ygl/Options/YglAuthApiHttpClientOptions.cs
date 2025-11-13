using FluentValidation;
using YourGamesList.Common.Options.Validators;

namespace YourGamesList.Web.Page.Services.Ygl.Options;

public class YglAuthApiHttpClientOptions
{
    public const string SectionName = "YglAuthApiHttpClient";

    public required string BaseAddress { get; init; }
}

public sealed class YglAuthApiHttpClientOptionsValidator : AbstractValidator<YglAuthApiHttpClientOptions>
{
    public YglAuthApiHttpClientOptionsValidator()
    {
        RuleFor(x => x.BaseAddress).IsValidAbsoluteUrl();
    }
}
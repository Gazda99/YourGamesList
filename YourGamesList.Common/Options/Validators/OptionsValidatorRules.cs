using System.Diagnostics.CodeAnalysis;

namespace YourGamesList.Common.Options.Validators;

[ExcludeFromCodeCoverage]
public static class OptionsValidatorRules
{
    public static bool IsValidUrl(string urlToCheck, UriKind uriKind = UriKind.Absolute)
    {
        return Uri.TryCreate(urlToCheck, uriKind, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
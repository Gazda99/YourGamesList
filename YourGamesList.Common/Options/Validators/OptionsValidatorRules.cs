using System;

namespace YourGamesList.Common.Options.Validators;

public static class OptionsValidatorRules
{
    public static bool IsValidAbsoluteUrl(string url)
    {
        return !string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
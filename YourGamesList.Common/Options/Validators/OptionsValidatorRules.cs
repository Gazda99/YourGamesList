namespace YourGamesList.Common.Options.Validators;

public static class OptionsValidatorRules
{
    public static bool IsValidUrl(string urlToCheck)
    {
        return Uri.TryCreate(urlToCheck, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
using Fluxor;

namespace YourGamesList.Web.Store.AuthFeature;

[FeatureState]
public class AuthState
{
    public string UserToken { get; init; } = string.Empty;
}
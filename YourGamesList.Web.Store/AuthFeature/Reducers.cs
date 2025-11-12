using Fluxor;

namespace YourGamesList.Web.Store.AuthFeature;

public class Reducers
{
    [ReducerMethod]
    public static AuthState ReduceIncrementCounterAction(AuthState state, LoginAction action)
    {
        return new AuthState()
        {
            UserToken = action.UserToken
        };
    }
}
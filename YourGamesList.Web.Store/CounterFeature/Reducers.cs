using Fluxor;

namespace YourGamesList.Web.Store.CounterFeature;

public static class Reducers
{
    [ReducerMethod]
    public static CounterState ReduceIncrementCounterAction(CounterState state, IncrementCounterAction action)
    {
        return new CounterState(state.ClickCount + 1);
    }
}
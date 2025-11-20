namespace YourGamesList.Web.Page.Services.StaticState;

public interface IStaticState<TState>
{
    TState? GetState();
    void SetState(TState state);
}
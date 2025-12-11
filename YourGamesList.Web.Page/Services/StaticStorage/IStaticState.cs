namespace YourGamesList.Web.Page.Services.StaticStorage;

public interface IStaticState<TState>
{
    TState? GetState();
    void SetState(TState state);
}
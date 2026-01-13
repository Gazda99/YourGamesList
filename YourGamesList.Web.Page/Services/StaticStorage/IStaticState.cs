namespace YourGamesList.Web.Page.Services.StaticStorage;

public interface IStaticState<TState>
{
    string StateName { get; }
    TState? GetState();
    void SetState(TState state);
}
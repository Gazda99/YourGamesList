using System.Threading.Tasks;

namespace YourGamesList.Web.Page.StaticState;

public interface IStaticState<TState>
{
    TState? GetState();
    void SetState(TState state);
}
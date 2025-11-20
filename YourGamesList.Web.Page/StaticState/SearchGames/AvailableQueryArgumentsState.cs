using YourGamesList.Contracts.Responses.Games;

namespace YourGamesList.Web.Page.StaticState.SearchGames;

public class AvailableQueryArgumentsState : IStaticState<AvailableSearchQueryArgumentsResponse>
{
    private AvailableSearchQueryArgumentsResponse? _state;

    public AvailableSearchQueryArgumentsResponse? GetState()
    {
        return _state;
    }

    public void SetState(AvailableSearchQueryArgumentsResponse state)
    {
        _state = state;
    }
}
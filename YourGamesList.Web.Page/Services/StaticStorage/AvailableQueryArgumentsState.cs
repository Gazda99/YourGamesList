using YourGamesList.Contracts.Responses.Games;

namespace YourGamesList.Web.Page.Services.StaticStorage;

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
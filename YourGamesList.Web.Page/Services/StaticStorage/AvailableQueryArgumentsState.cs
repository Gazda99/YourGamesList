using YourGamesList.Contracts.Responses.Games;

namespace YourGamesList.Web.Page.Services.StaticStorage;

public class AvailableQueryArgumentsState : IStaticState<AvailableSearchQueryArgumentsResponse>
{
    private AvailableSearchQueryArgumentsResponse? _state;

    public string StateName => $"{nameof(AvailableQueryArgumentsState)}:{nameof(AvailableSearchQueryArgumentsResponse)}";

    public AvailableSearchQueryArgumentsResponse? GetState()
    {
        return _state;
    }

    public void SetState(AvailableSearchQueryArgumentsResponse state)
    {
        _state = state;
    }

    public void RemoveState()
    {
        _state = null;
    }
}
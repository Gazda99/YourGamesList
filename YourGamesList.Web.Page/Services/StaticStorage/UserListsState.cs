using System.Collections.Generic;
using YourGamesList.Contracts.Dto;

namespace YourGamesList.Web.Page.Services.StaticStorage;

public class UserListsState : IStaticState<List<GamesListDto>>
{
    private List<GamesListDto>? _state;

    public string StateName => $"{nameof(UserListsState)}:List<GamesListDto>";

    public List<GamesListDto>? GetState()
    {
        return _state;
    }

    public void SetState(List<GamesListDto> state)
    {
        _state = state;
    }
}
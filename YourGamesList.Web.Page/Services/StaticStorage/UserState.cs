using YourGamesList.Contracts.Dto;

namespace YourGamesList.Web.Page.Services.StaticStorage;

public class UserState : IStaticState<UserDto>
{
    private UserDto? _state;

    public string StateName => $"{nameof(UserState)}:{nameof(UserDto)}";

    public UserDto? GetState()
    {
        return _state;
    }

    public void SetState(UserDto state)
    {
        _state = state;
    }
    
    public void RemoveState()
    {
        _state = null;
    }
}
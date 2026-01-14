using Microsoft.Extensions.Logging;

namespace YourGamesList.Web.Page.Services.StaticStorage;

public class LoggingStaticStateDecorator<TState> : IStaticState<TState>
{
    private readonly IStaticState<TState> _inner;
    private readonly ILogger<LoggingStaticStateDecorator<TState>> _logger;

    public LoggingStaticStateDecorator(
        IStaticState<TState> inner,
        ILogger<LoggingStaticStateDecorator<TState>> logger
    )
    {
        _inner = inner;
        _logger = logger;
    }

    public string StateName => _inner.StateName;

    public TState? GetState()
    {
        _logger.LogInformation("Retrieving state for {StateType}", StateName);

        var state = _inner.GetState();

        if (state == null)
        {
            _logger.LogInformation("State for {StateType} is null.", StateName);
        }
        else
        {
            _logger.LogInformation("State for {StateType} retrieved successfully.", StateName);
        }

        return state;
    }

    public void SetState(TState state)
    {
        _logger.LogInformation("Setting state for {StateType}.", StateName);

        _inner.SetState(state);

        _logger.LogInformation("State for {StateType} set successfully.", StateName);
    }
}
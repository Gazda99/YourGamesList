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

    public TState? GetState()
    {
        _logger.LogInformation("Retrieving state for {StateType}", typeof(TState).Name);

        var state = _inner.GetState();

        if (state == null)
        {
            _logger.LogInformation("State for {StateType} is null.", typeof(TState).Name);
        }
        else
        {
            _logger.LogInformation("State for {StateType} retrieved successfully.", typeof(TState).Name);
        }

        return state;
    }

    public void SetState(TState state)
    {
        _logger.LogInformation("Setting state for {StateType}.", typeof(TState).Name);

        _inner.SetState(state);

        _logger.LogInformation("State for {StateType} set successfully.", typeof(TState).Name);
    }
}
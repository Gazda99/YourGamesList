using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace YourGamesList.Api.Telemetry;

public interface IUserManagerServiceTelemetry
{
    void TrackSuccessfulLogin();
    void TrackFailedLogin(string failedLoginReason);
}

public class UserManagerServiceTelemetry : IUserManagerServiceTelemetry
{
    private const string MeterName = "YourGamesList.Api.UserManagerService";
    private const string LoggingAttemptCounterName = "user_login_attempts";
    private const string SuccessFlagName = "success";
    private const string FailedLoginReasonName = "reason";

    private readonly Counter<long> _loginAttemptCounter;

    public UserManagerServiceTelemetry(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(new MeterOptions(MeterName));

        _loginAttemptCounter = meter.CreateCounter<long>(LoggingAttemptCounterName, description: "Counts the number of login attempts");
    }

    public void TrackSuccessfulLogin()
    {
        var tags = new TagList
        {
            { SuccessFlagName, true }
        };
        _loginAttemptCounter.Add(1, tags);
    }

    public void TrackFailedLogin(string failedLoginReason)
    {
        var tags = new TagList
        {
            { SuccessFlagName, false },
            { FailedLoginReasonName, failedLoginReason }
        };
        _loginAttemptCounter.Add(1, tags);
    }
}
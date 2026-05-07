using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace YourGamesList.Api.Services.Users;

public interface IUsersServiceTelemetry
{
    void TrackSuccessfulLogin();
    void TrackFailedLogin(string failedLoginReason);
}

public class UsersServiceTelemetry : IUsersServiceTelemetry
{
    private const string LoggingAttemptCounterName = "user_login_attempts";
    private const string SuccessFlagName = "success";
    private const string FailedLoginReasonName = "reason";

    private readonly Counter<long> _loginAttemptCounter;


    public UsersServiceTelemetry(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(new MeterOptions("Ygl.OrderService")
        {
            Version = "1.0.0"
        });

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
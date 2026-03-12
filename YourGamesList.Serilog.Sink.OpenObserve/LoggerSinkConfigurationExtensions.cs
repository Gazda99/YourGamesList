using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Sinks.PeriodicBatching;

namespace YourGamesList.Serilog.Sink.OpenObserve;

public static class LoggerSinkConfigurationExtensions
{
    public static LoggerConfiguration OpenObserve(
        this LoggerSinkConfiguration loggerConfiguration,
        string url,
        string organization,
        string login,
        string secret,
        string streamName = "default",
        PeriodicBatchingSinkOptions? periodicBatchingSinkOptions = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentException.ThrowIfNullOrWhiteSpace(organization);
        ArgumentException.ThrowIfNullOrWhiteSpace(login);
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);

        periodicBatchingSinkOptions ??= new PeriodicBatchingSinkOptions();

        var sink = new OpenObserveSink(url, organization, login, secret, streamName);

        return loggerConfiguration.Sink(new PeriodicBatchingSink(sink, periodicBatchingSinkOptions));
    }
}
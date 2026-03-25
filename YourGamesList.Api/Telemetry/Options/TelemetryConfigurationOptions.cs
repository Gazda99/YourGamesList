using FluentValidation;

namespace YourGamesList.Api.Telemetry.Options;

public class TelemetryConfigurationOptions
{
    public const string SectionName = "TelemetryConfiguration";

    public required string ApiSecret { get; init; }
    public required string OtlpExporterTracesEndpoint { get; init; }
    public required string OtlpExporterMetricsEndpoint { get; init; }
    public required string ServiceName { get; init; }
    public required string TracesStreamName { get; init; }
}

public class TelemetryConfigurationOptionsValidator : AbstractValidator<TelemetryConfigurationOptions>
{
    public TelemetryConfigurationOptionsValidator()
    {
        RuleFor(x => x.ApiSecret).NotEmpty();
        RuleFor(x => x.OtlpExporterTracesEndpoint).NotEmpty();
        RuleFor(x => x.OtlpExporterMetricsEndpoint).NotEmpty();
        RuleFor(x => x.ServiceName).NotEmpty();
        RuleFor(x => x.TracesStreamName).NotEmpty();
    }
}
using FluentValidation;

namespace YourGamesList.Api.Services.HealthChecks.Options;

public class HealthCheckServiceOptions
{
    public const string SectionName = "HealthCheckService";

    public int TimeoutInSeconds { get; set; } = 15;
    public int CacheDurationInSeconds { get; set; } = 30;
}

internal sealed class HealthCheckServiceOptionsValidator : AbstractValidator<HealthCheckServiceOptions>
{
    public HealthCheckServiceOptionsValidator()
    {
        RuleFor(x => x.TimeoutInSeconds).GreaterThan(0);
        RuleFor(x => x.CacheDurationInSeconds)
            .GreaterThan(0)
            .GreaterThan(x => x.TimeoutInSeconds);
    }
}
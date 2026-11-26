using System;
using FluentValidation;
using Microsoft.Extensions.Http.Resilience;

namespace YourGamesList.Common.Http;

/// <summary>
/// Resilience settings for a single HTTP client. Meant to be exposed as a "Resilience" property of the client's own options class.
/// Only a total request timeout is supported for now; further strategies (retry, circuit breaker) can be added here.
/// </summary>
public class HttpResilienceOptions
{
    /// <summary>
    /// Timeout for the whole outgoing call. Optional; when not configured, the default of 30 seconds is used.
    /// </summary>
    public HttpTimeoutStrategyOptions? TotalRequestTimeout { get; init; }
}

public class HttpResilienceOptionsValidator : AbstractValidator<HttpResilienceOptions>
{
    // Range enforced by Polly's timeout strategy; validated here so misconfiguration fails at startup.
    public static readonly TimeSpan MinimumTimeout = TimeSpan.FromMilliseconds(10);
    public static readonly TimeSpan MaximumTimeout = TimeSpan.FromHours(24);

    public HttpResilienceOptionsValidator()
    {
        RuleFor(x => x.TotalRequestTimeout!)
            .ChildRules(timeout =>
            {
                timeout.RuleFor(x => x.Timeout)
                    .InclusiveBetween(MinimumTimeout, MaximumTimeout);
            })
            .When(x => x.TotalRequestTimeout is not null);
    }
}

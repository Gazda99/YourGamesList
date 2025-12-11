using FluentValidation;

namespace YourGamesList.Api.Services.CorrelationId.Options;

public class CorrelationIdMiddlewareOptions
{
    public const string SectionName = "CorrelationIdMiddleware";
    public required bool ReadCorrelationIdFromRequestHeader { get; init; } = false;
}

internal sealed class CorrelationIdMiddlewareOptionsValidator : AbstractValidator<CorrelationIdMiddlewareOptions>
{
    public CorrelationIdMiddlewareOptionsValidator()
    {
        //Empty for now
    }
}
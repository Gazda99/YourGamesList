using System.Collections.Generic;
using FluentValidation;

namespace YourGamesList.Api.Services.Auth.Options;

public class ApiKeysOptions
{
    public const string SectionName = "ApiKeys";

    public required IDictionary<string, string> Keys { get; init; }
}

internal sealed class ApiKeysOptionsValidator : AbstractValidator<ApiKeysOptions>
{
    public ApiKeysOptionsValidator()
    {
        //Intentionally left empty
    }
}
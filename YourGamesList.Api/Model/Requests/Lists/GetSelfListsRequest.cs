using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Lists;

public class GetSelfListsRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }
    [FromQuery] public bool? IncludeGames { get; init; } = false;
}

internal sealed class GetSelfListsRequestValidator : AbstractValidator<GetSelfListsRequest>
{
    public GetSelfListsRequestValidator(IValidator<JwtUserInformation> jwtUserInformationValidator)
    {
        RuleFor(x => x.UserInformation).SetValidator(jwtUserInformationValidator);
    }
}
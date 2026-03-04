using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Lists;

public class GetSelfListsRequest
{
    [FromAuthorizeHeader] public required UserInformationToken UserInformation { get; init; }
    [FromQuery] public bool? IncludeGames { get; init; } = false;
}

internal sealed class GetSelfListsRequestValidator : AbstractValidator<GetSelfListsRequest>
{
    public GetSelfListsRequestValidator(IValidator<UserInformationToken> jwtUserInformationValidator)
    {
        RuleFor(x => x.UserInformation).SetValidator(jwtUserInformationValidator);
    }
}
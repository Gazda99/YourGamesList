using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Lists;

public class GetSelfListsRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }
    [FromQuery] public bool IncludeGames { get; init; } = false;
}

//TODO: unit tests
internal sealed class GetSelfListsRequestValidator : AbstractValidator<GetSelfListsRequest>
{
    public GetSelfListsRequestValidator()
    {
        RuleFor(x => x.UserInformation).SetValidator(new JwtUserInformationValidator());
    }
}
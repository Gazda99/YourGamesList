using System;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Lists;

public class GetListRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }

    [FromRoute(Name = "listId")] public required Guid ListId { get; init; }
    [FromQuery] public bool? IncludeGames { get; init; } = false;
}

internal sealed class GetListRequestValidator : AbstractValidator<GetListRequest>
{
    public GetListRequestValidator(IValidator<JwtUserInformation> jwtUserInformationValidator)
    {
        RuleFor(x => x.UserInformation).SetValidator(jwtUserInformationValidator);
        RuleFor(x => x.ListId).NotNull().NotEmpty();
    }
}
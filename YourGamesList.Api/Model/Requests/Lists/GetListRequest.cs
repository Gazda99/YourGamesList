using System;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Lists;

public class GetListRequest
{
    [FromAuthorizeHeader] public required UserInformationToken UserInformation { get; init; }
    [FromRoute] public required Guid ListId { get; init; }
}

public class GetListRequestValidator : AbstractValidator<GetListRequest>
{
    public GetListRequestValidator()
    {
        RuleFor(x => x.ListId)
            .NotEmpty();

        RuleFor(x => x.UserInformation)
            .NotEmpty();
    }
}

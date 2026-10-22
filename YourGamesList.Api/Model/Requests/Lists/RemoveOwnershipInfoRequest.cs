using System;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Lists;

public class RemoveOwnershipInfoRequest
{
    [FromAuthorizeHeader] public required UserInformationToken UserInformation { get; init; }
    [FromRoute] public required Guid ListId { get; init; }
    [FromRoute] public required Guid EntryId { get; init; }
    [FromRoute] public required Guid OwnershipInfoId { get; init; }
}

public class RemoveOwnershipInfoRequestValidator : AbstractValidator<RemoveOwnershipInfoRequest>
{
    public RemoveOwnershipInfoRequestValidator()
    {
        RuleFor(request => request.ListId).NotEmpty();
        RuleFor(request => request.EntryId).NotEmpty();
        RuleFor(request => request.OwnershipInfoId).NotEmpty();
        RuleFor(request => request.UserInformation).NotEmpty();
    }
}

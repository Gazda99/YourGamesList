using System;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Dto;

namespace YourGamesList.Api.Model.Requests.Lists;

public class UpdateEntriesInListRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }
    [FromBody] public required UpdateEntriesInListRequestBody Body { get; init; }
}

public class UpdateEntriesInListRequestBody
{
    public required Guid ListId { get; init; }
    public EntryToUpdateRequestPart[] EntriesToUpdate { get; init; } = [];
}

public class EntryToUpdateRequestPart
{
    public required Guid EntryId { get; init; }

    public string? Desc { get; set; }
    public PlatformDto[]? Platforms { get; set; }
    public GameDistributionDto[]? GameDistributions { get; set; }
    public bool? IsStarred { get; set; }
    public byte? Rating { get; set; }
    public CompletionStatusDto? CompletionStatus { get; set; }
}

internal sealed class UpdateEntriesInListRequestValidator : AbstractValidator<UpdateEntriesInListRequest>
{
    public UpdateEntriesInListRequestValidator(IValidator<JwtUserInformation> jwtUserInformationValidator)
    {
        RuleFor(x => x.UserInformation).SetValidator(jwtUserInformationValidator);

        RuleFor(x => x.Body.ListId)
            .NotEmpty()
            .WithMessage("List Id is required.");

        RuleForEach(x => x.Body.EntriesToUpdate)
            .ChildRules(entry =>
            {
                entry.RuleFor(x => x.EntryId)
                    .NotEmpty()
                    .WithMessage("Entry Id is required.");
            });
    }
}
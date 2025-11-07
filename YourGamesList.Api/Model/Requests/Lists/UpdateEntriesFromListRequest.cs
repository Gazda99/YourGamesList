using System;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Dto;

namespace YourGamesList.Api.Model.Requests.Lists;

public class UpdateEntriesFromListRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }
    [FromBody] public required UpdateEntriesToListRequestBody Body { get; init; }
}

public class UpdateEntriesToListRequestBody
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

//TODO: unit tests
internal sealed class UpdateEntriesFromListRequestValidator : AbstractValidator<UpdateEntriesFromListRequest>
{
    public UpdateEntriesFromListRequestValidator()
    {
        RuleFor(x => x.UserInformation).SetValidator(new JwtUserInformationValidator());
    }
}
using System;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Dto;

namespace YourGamesList.Api.Model.Requests.Lists;

public class AddEntriesToListRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }
    [FromBody] public required AddEntriesToListRequestBody Body { get; init; }
}

public class AddEntriesToListRequestBody
{
    public required Guid ListId { get; init; }
    public EntryToAddRequestPart[] EntriesToAdd { get; init; } = [];
}

public class EntryToAddRequestPart
{
    public required Guid GameId { get; init; }

    public string? Desc { get; set; }
    public PlatformDto[]? Platforms { get; set; }
    public GameDistributionDto[]? GameDistributions { get; set; }
    public bool? IsStarred { get; set; }
    public byte? Rating { get; set; }
    public CompletionStatusDto? CompletionStatus { get; set; }
}

internal sealed class AddEntriesToListRequestValidator : AbstractValidator<AddEntriesToListRequest>
{
    public AddEntriesToListRequestValidator(IValidator<JwtUserInformation> jwtUserInformationValidator)
    {
        RuleFor(x => x.UserInformation).SetValidator(jwtUserInformationValidator);

        RuleFor(x => x.Body.ListId)
            .NotEmpty()
            .WithMessage("List Id is required.");

        RuleForEach(x => x.Body.EntriesToAdd)
            .ChildRules(entry =>
            {
                entry.RuleFor(x => x.GameId)
                    .NotEmpty()
                    .WithMessage("Game Id is required.");
            });
    }
}
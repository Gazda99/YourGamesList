using System;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Lists;

public class UpdateListsRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }

    [FromBody] public required UpdateListsRequestBody Body { get; init; }
}

public class UpdateListsRequestBody
{
    public Guid ListId { get; init; }
    public string? Name { get; set; }
    public string? Desc { get; set; }
    public bool? IsPublic { get; set; }
}

//TODO: unit tests
internal sealed class UpdateListsRequestValidator : AbstractValidator<UpdateListsRequest>
{
    public UpdateListsRequestValidator()
    {
        RuleFor(x => x.UserInformation).SetValidator(new JwtUserInformationValidator());
    }
}
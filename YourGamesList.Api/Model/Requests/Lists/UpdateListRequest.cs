using System;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Lists;

public class UpdateListRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }

    [FromBody] public required UpdateListRequestBody Body { get; init; }
}

public class UpdateListRequestBody
{
    public Guid ListId { get; init; }
    public string? Name { get; set; }
    public string? Desc { get; set; }
    public bool? IsPublic { get; set; }
}

//TODO: unit tests
internal sealed class UpdateListRequestValidator : AbstractValidator<UpdateListRequest>
{
    public UpdateListRequestValidator()
    {
        RuleFor(x => x.UserInformation).SetValidator(new JwtUserInformationValidator());
    }
}
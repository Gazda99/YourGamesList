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
    public required Guid ListId { get; init; }
    public string? Name { get; set; }
    public string? Desc { get; set; }
    public bool? IsPublic { get; set; }
}

internal sealed class UpdateListRequestValidator : AbstractValidator<UpdateListRequest>
{
    public UpdateListRequestValidator(IValidator<JwtUserInformation> jwtUserInformationValidator)
    {
        RuleFor(x => x.UserInformation).SetValidator(jwtUserInformationValidator);

        RuleFor(x => x.Body)
            .NotEmpty()
            .WithMessage("Request body is empty.");

        RuleFor(x => x.Body.ListId)
            .NotEmpty()
            .WithMessage("List Id is required.");
    }
}
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Lists;

public class CreateListRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }

    [FromBody] public required CreateListRequestBody Body { get; init; }
}

public class CreateListRequestBody
{
    public required string ListName { get; init; } = string.Empty;
    public string? Description { get; init; } = string.Empty;
}

internal sealed class CreateListRequestValidator : AbstractValidator<CreateListRequest>
{
    public CreateListRequestValidator(IValidator<JwtUserInformation> jwtUserInformationValidator)
    {
        RuleFor(x => x.UserInformation).SetValidator(jwtUserInformationValidator);

        RuleFor(x => x.Body)
            .NotEmpty()
            .WithMessage("Request body is empty.");

        RuleFor(x => x.Body!.ListName)
            .NotEmpty()
            .WithMessage("List name is required.");
    }
}
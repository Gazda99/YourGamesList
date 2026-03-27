using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;
using YourGamesList.Contracts.Requests.Lists;

namespace YourGamesList.Api.Model.Requests.Lists;

public class CreateListRequest
{
    [FromAuthorizeHeader] public required UserInformationToken UserInformation { get; init; }

    [FromBody] public required CreateListRequestBody Body { get; init; }
}

internal sealed class CreateListRequestValidator : AbstractValidator<CreateListRequest>
{
    public CreateListRequestValidator(IValidator<UserInformationToken> jwtUserInformationValidator)
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
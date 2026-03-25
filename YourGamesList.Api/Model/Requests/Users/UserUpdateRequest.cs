using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;
using YourGamesList.Contracts.Requests.Users;

namespace YourGamesList.Api.Model.Requests.Users;

public class UserUpdateRequest
{
    [FromAuthorizeHeader] public required UserInformationToken UserInformation { get; init; }
    [FromBody] public required UserUpdateRequestBody Body { get; init; }
}

internal sealed class UserUpdateRequestValidator : AbstractValidator<UserUpdateRequest>
{
    public UserUpdateRequestValidator(IValidator<UserInformationToken> jwtUserInformationValidator)
    {
        RuleFor(x => x.UserInformation).SetValidator(jwtUserInformationValidator);

        RuleFor(x => x.Body)
            .NotEmpty()
            .WithMessage("Request body is empty.");
    }
}
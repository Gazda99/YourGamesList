using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;
using YourGamesList.Contracts.Requests.Users;

namespace YourGamesList.Api.Model.Requests.Users;

public class UserUpdateRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }
    [FromBody] public required UserUpdateRequestBody Body { get; init; }
}

internal sealed class UserUpdateRequestValidator : AbstractValidator<UserUpdateRequest>
{
    public UserUpdateRequestValidator(IValidator<JwtUserInformation> jwtUserInformationValidator)
    {
        RuleFor(x => x.UserInformation).SetValidator(jwtUserInformationValidator);

        RuleFor(x => x.Body)
            .NotEmpty()
            .WithMessage("Request body is empty.");
    }
}
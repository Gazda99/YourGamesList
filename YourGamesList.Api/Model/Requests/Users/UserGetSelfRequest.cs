using FluentValidation;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Users;

public class UserGetSelfRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }
}

internal sealed class UserGetSelfRequestValidator : AbstractValidator<UserGetSelfRequest>
{
    public UserGetSelfRequestValidator(IValidator<JwtUserInformation> jwtUserInformationValidator)
    {
        RuleFor(x => x.UserInformation).SetValidator(jwtUserInformationValidator);
    }
}
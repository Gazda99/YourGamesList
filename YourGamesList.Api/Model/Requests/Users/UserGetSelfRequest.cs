using FluentValidation;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Users;

public class UserGetSelfRequest
{
    [FromAuthorizeHeader] public required UserInformationToken UserInformation { get; init; }
}

internal sealed class UserGetSelfRequestValidator : AbstractValidator<UserGetSelfRequest>
{
    public UserGetSelfRequestValidator(IValidator<UserInformationToken> jwtUserInformationValidator)
    {
        RuleFor(x => x.UserInformation).SetValidator(jwtUserInformationValidator);
    }
}
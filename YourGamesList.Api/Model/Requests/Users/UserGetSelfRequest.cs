using FluentValidation;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Users;

public class UserGetSelfRequest
{
    [FromAuthorizeHeader] public required UserInformationToken UserInformation { get; set; }
}

public class UserGetSelfRequestValidator : AbstractValidator<UserGetSelfRequest>
{
    public UserGetSelfRequestValidator()
    {
        RuleFor(x => x.UserInformation)
            .NotEmpty();
    }
}
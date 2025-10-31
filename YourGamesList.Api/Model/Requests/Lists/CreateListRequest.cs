using FluentValidation;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Lists;

public class CreateListRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }
}

//TODO: unit tests
internal sealed class CreateListRequestValidator : AbstractValidator<CreateListRequest>
{
    public CreateListRequestValidator()
    {
        RuleFor(x => x.UserInformation).SetValidator(new JwtUserInformationValidator());
    }
}
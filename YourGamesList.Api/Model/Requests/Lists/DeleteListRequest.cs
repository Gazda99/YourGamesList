using FluentValidation;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Lists;

public class DeleteListRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }
}

//TODO: unit tests
internal sealed class DeleteListRequestValidator : AbstractValidator<DeleteListRequest>
{
    public DeleteListRequestValidator()
    {
        RuleFor(x => x.UserInformation).SetValidator(new JwtUserInformationValidator());
    }
}
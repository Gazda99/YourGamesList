using FluentValidation;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Lists;

public class UpdateListRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }
}

//TODO: unit tests
internal sealed class UpdateListRequestValidator : AbstractValidator<UpdateListRequest>
{
    public UpdateListRequestValidator()
    {
        RuleFor(x => x.UserInformation).SetValidator(new JwtUserInformationValidator());
    }
}
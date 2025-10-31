using FluentValidation;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Lists;

public class GetListsRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }
}

//TODO: unit tests
internal sealed class GetListsRequestValidator : AbstractValidator<GetListsRequest>
{
    public GetListsRequestValidator()
    {
        RuleFor(x => x.UserInformation).SetValidator(new JwtUserInformationValidator());
    }
}
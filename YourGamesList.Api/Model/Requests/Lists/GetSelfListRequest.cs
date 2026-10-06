using FluentValidation;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Lists;

public class GetSelfListRequest
{
    [FromAuthorizeHeader] public required UserInformationToken UserInformation { get; init; }
}

public class GetSelfListRequestValidator : AbstractValidator<GetSelfListRequest>
{
    public GetSelfListRequestValidator()
    {
        RuleFor(x => x.UserInformation)
            .NotEmpty();
    }
}
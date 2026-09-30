using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;
using YourGamesList.Contracts.Requests.Lists;

namespace YourGamesList.Api.Model.Requests.Lists;

public class CreateListRequest
{
    [FromAuthorizeHeader] public required UserInformationToken UserInformation { get; init; }
    [FromBody] public required CreateListRequestBody Body { get; init; }
}

public class CreateListRequestValidator : AbstractValidator<CreateListRequest>
{
    public CreateListRequestValidator()
    {
        RuleFor(x => x.Body)
            .NotEmpty()
            .WithMessage("Proper request body is required.")
            .ChildRules(body =>
            {
                body.RuleFor(x => x.Name)
                    .NotEmpty();
            });
    }
}
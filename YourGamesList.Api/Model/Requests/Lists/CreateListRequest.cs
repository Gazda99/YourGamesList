using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Lists;

public class CreateListRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }

    [FromBody] public ListCreateRequestBody? Body { get; init; }
}

public class ListCreateRequestBody
{
    public string ListName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

//TODO: unit tests
internal sealed class CreateListRequestValidator : AbstractValidator<CreateListRequest>
{
    public CreateListRequestValidator()
    {
        RuleFor(x => x.UserInformation).SetValidator(new JwtUserInformationValidator());
        RuleFor(x => x.Body)
            .NotEmpty()
            .WithMessage("Request body is empty");
        When(x => x.Body != null, () =>
        {
            RuleFor(x => x.Body!.ListName)
                .NotEmpty()
                .WithMessage("List name is required");
        });
    }
}
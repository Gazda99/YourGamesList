using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Lists;

public class DeleteListRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }
    [FromQuery(Name = "listName")] public string ListName { get; init; } = string.Empty;
}

//TODO: unit tests
internal sealed class DeleteListRequestValidator : AbstractValidator<DeleteListRequest>
{
    public DeleteListRequestValidator()
    {
        RuleFor(x => x.UserInformation).SetValidator(new JwtUserInformationValidator());
        RuleFor(x => x.ListName).NotEmpty();
    }
}
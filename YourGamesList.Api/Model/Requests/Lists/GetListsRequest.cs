using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Lists;

public class GetListsRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }
    [FromQuery(Name = "listName")] public string ListName { get; init; } = string.Empty;
}

//TODO: unit tests
internal sealed class GetListsRequestValidator : AbstractValidator<GetListsRequest>
{
    public GetListsRequestValidator()
    {
        RuleFor(x => x.UserInformation).SetValidator(new JwtUserInformationValidator());
        RuleFor(x => x.ListName).NotEmpty();
    }
}
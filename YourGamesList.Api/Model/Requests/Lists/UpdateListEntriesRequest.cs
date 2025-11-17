using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;
using YourGamesList.Contracts.Requests.Lists;

namespace YourGamesList.Api.Model.Requests.Lists;

public class UpdateListEntriesRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }
    [FromBody] public required UpdateListEntriesRequestBody Body { get; init; }
}

internal sealed class UpdateListEntriesRequestValidator : AbstractValidator<UpdateListEntriesRequest>
{
    public UpdateListEntriesRequestValidator(IValidator<JwtUserInformation> jwtUserInformationValidator)
    {
        RuleFor(x => x.UserInformation).SetValidator(jwtUserInformationValidator);

        RuleFor(x => x.Body.ListId)
            .NotEmpty()
            .WithMessage("List Id is required.");

        RuleForEach(x => x.Body.EntriesToUpdate)
            .ChildRules(entry =>
            {
                entry.RuleFor(x => x.EntryId)
                    .NotEmpty()
                    .WithMessage("Entry Id is required.");
            });
    }
}
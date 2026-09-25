using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace YourGamesList.Api.Model.Requests.Users;

public class ConfirmEmailRequest
{
    [FromQuery(Name = "token")] public string Token { get; init; } = "";
}

public class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
{
    public ConfirmEmailRequestValidator()
    {
    }
}
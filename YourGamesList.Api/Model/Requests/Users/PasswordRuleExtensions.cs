using FluentValidation;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.Model.Requests.Users;

public static class PasswordRuleExtensions
{
    /// <summary>
    /// Rules for a password that is about to be set (registration, password change): required and within configured length bounds.
    /// </summary>
    public static IRuleBuilderOptions<T, string> MustBeValidNewPassword<T>(
        this IRuleBuilderInitial<T, string> ruleBuilder,
        PasswordValidatorOptions passwordOptions,
        ILogger logger)
    {
        var lengthMessage = $"Password must be between {passwordOptions.MinimumPasswordLength} and {passwordOptions.MaximumPasswordLength} characters long.";

        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.UserAuth.InvalidPassword)
            .WithMessage("Password is required.")
            .Must(password =>
            {
                if (password.Length < passwordOptions.MinimumPasswordLength)
                {
                    logger.LogInformation("Password is too short. {PasswordLength} < {PasswordMinimumLength}", password.Length, passwordOptions.MinimumPasswordLength);
                    return false;
                }
                return true;
            })
            .WithErrorCode(ErrorCodes.UserAuth.PasswordIsTooShort)
            .WithMessage(lengthMessage)
            .Must(password =>
            {
                if (password.Length > passwordOptions.MaximumPasswordLength)
                {
                    logger.LogInformation("Password is too long. {PasswordLength} > {PasswordMaximumLength}", password.Length, passwordOptions.MaximumPasswordLength);
                    return false;
                }
                return true;
            })
            .WithErrorCode(ErrorCodes.UserAuth.PasswordIsTooLong)
            .WithMessage(lengthMessage);
    }
}

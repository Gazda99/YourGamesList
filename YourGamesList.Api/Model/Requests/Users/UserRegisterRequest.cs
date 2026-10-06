using System;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Contracts.Requests.Users;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.Model.Requests.Users;

public class UserRegisterRequest
{
    [FromBody] public required AuthUserRegisterRequestBody Body { get; init; }
}

public class UserRegisterRequestValidator : AbstractValidator<UserRegisterRequest>
{
    public UserRegisterRequestValidator(
        ILogger<UserRegisterRequestValidator> logger,
        IOptions<UserValuesValidatorOptions> options,
        TimeProvider timeProvider
    )
    {
        var validatorOptions = options.Value;
        
        RuleFor(x => x.Body)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Validation.RequestBodyRequired)
            .WithMessage("Proper request body is required.")
            .ChildRules(body =>
            {
                body.RuleFor(x => x.Username)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithErrorCode(ErrorCodes.UserAuth.InvalidUsername)
                    .WithMessage("Username is required.")
                    .Must(username => username.Length >= validatorOptions.Username.MinimumUsernameLength)
                    .WithErrorCode(ErrorCodes.UserAuth.UsernameIsTooShort)
                    .WithMessage($"Username must be between {validatorOptions.Username.MinimumUsernameLength} and {validatorOptions.Username.MaximumUsernameLength} characters long.")
                    .Must(username => username.Length <= validatorOptions.Username.MaximumUsernameLength)
                    .WithErrorCode(ErrorCodes.UserAuth.UsernameIsTooLong)
                    .WithMessage($"Username must be between {validatorOptions.Username.MinimumUsernameLength} and {validatorOptions.Username.MaximumUsernameLength} characters long.")
                    .Must(username => Regex.IsMatch(username, validatorOptions.Username.AllowedCharactersRegex))
                    .WithErrorCode(ErrorCodes.UserAuth.InvalidUsername)
                    .WithMessage("Username contains invalid characters.")
                    ;

                body.RuleFor(x => x.Email)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithErrorCode(ErrorCodes.UserAuth.InvalidEmail)
                    .WithMessage("Invalid email address.")
                    .EmailAddress()
                    .WithErrorCode(ErrorCodes.UserAuth.InvalidEmail)
                    .WithMessage("Invalid email address.")
                    .Must(email => email.Length <= validatorOptions.Email.MaximumEmailLength)
                    .WithErrorCode(ErrorCodes.UserAuth.EmailIsTooLong)
                    .WithMessage($"Username must be between {validatorOptions.Username.MinimumUsernameLength} and {validatorOptions.Username.MaximumUsernameLength} characters long.")
                    .Must(email =>
                    {
                        var localPart = email.Split("@").FirstOrDefault();
                        if (string.IsNullOrWhiteSpace(localPart) || localPart.Length > 64)
                        {
                            logger.LogInformation("{EmailLocalPart} is invalid email local part.", localPart);
                            return false;
                        }

                        return true;
                    })
                    .WithErrorCode(ErrorCodes.UserAuth.InvalidEmail)
                    .WithMessage("Invalid email address.")
                    ;

                body.RuleFor(x => x.Password)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithErrorCode(ErrorCodes.UserAuth.InvalidPassword)
                    .WithMessage("Password is required.")
                    .Must(username => username.Length >= validatorOptions.Username.MinimumUsernameLength)
                    .WithErrorCode(ErrorCodes.UserAuth.PasswordIsTooShort)
                    .WithMessage($"Password must be between {validatorOptions.Username.MinimumUsernameLength} and {validatorOptions.Username.MaximumUsernameLength} characters long.")
                    .Must(username => username.Length <= validatorOptions.Username.MaximumUsernameLength)
                    .WithErrorCode(ErrorCodes.UserAuth.PasswordIsTooLong)
                    .WithMessage($"Password must be between {validatorOptions.Username.MinimumUsernameLength} and {validatorOptions.Username.MaximumUsernameLength} characters long.")
                    ;
                
                var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
                
                body.RuleFor(x => x.DateOfBirth)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithErrorCode(ErrorCodes.UserAuth.InvalidDateOfBirth)
                    .WithMessage("Date of birth is required.")
                    .Must(dob =>
                    {
                        if (dob > today)
                        {
                            logger.LogInformation("date of birth is in the future.");
                            return false;
                        }

                        return true;
                    })
                    .WithErrorCode(ErrorCodes.UserAuth.InvalidDateOfBirth)
                    .WithMessage("Invalid date of birth.")
                    .Must(dob =>
                    {
                        var minimumAge = validatorOptions.DateOfBirth.MinimumAge;
                        var cutoffDate = today.AddYears(-minimumAge);

                        if (dob > cutoffDate)
                        {
                            logger.LogInformation("date of birth does not meet minimum age restrictions.");
                            return false;
                        }

                        return true;
                    })
                    .WithErrorCode(ErrorCodes.UserAuth.UnderMinimumAge)
                    .WithMessage("Under minimum age.")
                    .Must(dob =>
                    {
                        var maximumAge = validatorOptions.DateOfBirth.MaximumAge;

                        var oldestAllowedDate = today.AddYears(-maximumAge);
                        if (dob < oldestAllowedDate) 
                        {
                            logger.LogInformation("date of birth exceeds maximum age restrictions.");
                            return false;
                        }

                        return true;
                    })
                    .WithErrorCode(ErrorCodes.UserAuth.ExceedsMaximumAge)
                    .WithMessage("Invalid date of birth.")
                    ;
            });
    }
}
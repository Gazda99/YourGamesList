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
            .NotNull()
            .WithErrorCode(ErrorCodes.Validation.RequestBodyRequired)
            .WithMessage("Proper request body is required.")
            .ChildRules(body =>
            {
                body.RuleFor(x => x.Username)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithErrorCode(ErrorCodes.UserAuth.InvalidUsername)
                    .WithMessage("Username is required.")
                    .Must(username =>
                    {
                        if (username.Length < validatorOptions.Username.MinimumUsernameLength)
                        {
                            logger.LogInformation("Username {Username} is too short. {UsernameLength} < {UsernameMinimumLength} ", username, username.Length, validatorOptions.Username.MinimumUsernameLength);
                            return false;
                        }
                        return true;
                    })
                    .WithErrorCode(ErrorCodes.UserAuth.UsernameIsTooShort)
                    .WithMessage($"Username must be between {validatorOptions.Username.MinimumUsernameLength} and {validatorOptions.Username.MaximumUsernameLength} characters long.")
                    .Must(username =>
                    {
                        if (username.Length > validatorOptions.Username.MaximumUsernameLength)
                        {
                            logger.LogInformation("Username {Username} is too long. {UsernameLength} > {UsernameMaximumLength} ", username, username.Length, validatorOptions.Username.MaximumUsernameLength);
                            return false;
                        }
                        return true;
                    })
                    .WithErrorCode(ErrorCodes.UserAuth.UsernameIsTooLong)
                    .WithMessage($"Username must be between {validatorOptions.Username.MinimumUsernameLength} and {validatorOptions.Username.MaximumUsernameLength} characters long.")
                    .Must(username =>
                    {
                        if (!Regex.IsMatch(username, validatorOptions.Username.AllowedCharactersRegex))
                        {
                            logger.LogInformation("Username {Username} doesn't match allowed characters regex.", username);
                            return false;
                        }
                        return true;
                     
                    })
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
                    .Must(email =>
                    {
                        if (email.Length > validatorOptions.Email.MaximumEmailLength)
                        {
                            logger.LogInformation("Email {Email} is too long. {EmailLength} > {EmailMaximumLength} ", email, email.Length, validatorOptions.Email.MaximumEmailLength);
                            return false;
                        }
                        return true;
                    })
                    .WithErrorCode(ErrorCodes.UserAuth.EmailIsTooLong)
                    .WithMessage($"Email must be max {validatorOptions.Email.MaximumEmailLength} characters long.")
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
                    .Must(password =>
                    {
                        if (password.Length < validatorOptions.Password.MinimumPasswordLength)
                        {
                            logger.LogInformation("Password {Password} is too short. {PasswordLength} < {PasswordMinimumLength} ", password, password.Length, validatorOptions.Password.MinimumPasswordLength);
                            return false;
                        }
                        return true;
                    })
                    .WithErrorCode(ErrorCodes.UserAuth.PasswordIsTooShort)
                    .WithMessage($"Password must be between {validatorOptions.Password.MinimumPasswordLength} and {validatorOptions.Password.MaximumPasswordLength} characters long.")
                    .Must(password =>
                    {
                        if (password.Length > validatorOptions.Password.MaximumPasswordLength)
                        {
                            logger.LogInformation("Password {Password} is too long. {PasswordLength} > {PasswordMaximumLength} ", password, password.Length, validatorOptions.Password.MaximumPasswordLength);
                            return false;
                        }
                        return true;
                    })
                    .WithErrorCode(ErrorCodes.UserAuth.PasswordIsTooLong)
                    .WithMessage($"Password must be between {validatorOptions.Password.MinimumPasswordLength} and {validatorOptions.Password.MaximumPasswordLength} characters long.")
                    ;
                
                body.RuleFor(x => x.DateOfBirth)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                    .WithErrorCode(ErrorCodes.UserAuth.InvalidDateOfBirth)
                    .WithMessage("Date of birth is required.")
                    .Must(dob =>
                    {
                        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
                        if (dob > today)
                        {
                            logger.LogInformation("date of birth is in the future.");
                            return false;
                        }

                        return true;
                    })
                    .WithErrorCode(ErrorCodes.UserAuth.InvalidDateOfBirth)
                    .WithMessage("Invalid Date of birth.")
                    .Must(dob =>
                    {
                        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
                        var minimumAge = validatorOptions.DateOfBirth.MinimumAge;
                        var cutoffDate = today.AddYears(-minimumAge);

                        if (dob > cutoffDate)
                        {
                            logger.LogInformation("Date of birth does not meet minimum age restrictions.");
                            return false;
                        }

                        return true;
                    })
                    .WithErrorCode(ErrorCodes.UserAuth.UnderMinimumAge)
                    .WithMessage("Under minimum age.")
                    .Must(dob =>
                    {
                        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
                        var maximumAge = validatorOptions.DateOfBirth.MaximumAge;

                        var oldestAllowedDate = today.AddYears(-maximumAge);
                        if (dob < oldestAllowedDate) 
                        {
                            logger.LogInformation("Date of birth exceeds maximum age restrictions.");
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
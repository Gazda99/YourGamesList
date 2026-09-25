using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourGamesList.Api.Services.Auth.Model;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Common;

namespace YourGamesList.Api.Services.Auth;

public interface IUserValuesValidator
{
    /// <summary>
    /// Validates the string value of <paramref name="email"/>. It does not check the DB availability!
    /// </summary>
    ErrorResult<UserAuthError> ValidateEmail(string email);

    /// <summary>
    /// Validates the string value of <paramref name="username"/>. It does not check the DB availability!
    /// </summary>
    ErrorResult<UserAuthError> ValidateUsername(string username);

    /// <summary>
    /// Validates the string value of <paramref name="password"/>.
    /// </summary>
    ErrorResult<UserAuthError> ValidatePassword(string password);

    /// <summary>
    /// Validates the value of <paramref name="dateOfBirth"/>.
    /// </summary>
    ErrorResult<UserAuthError> ValidateDateOfBirth(DateOnly dateOfBirth);
}

public class UserValuesValidator : IUserValuesValidator
{
    private readonly ILogger<UserValuesValidator> _logger;
    private readonly IOptions<UserValuesValidatorOptions> _options;
    private readonly TimeProvider _timeProvider;

    public UserValuesValidator(
        ILogger<UserValuesValidator> logger,
        IOptions<UserValuesValidatorOptions> options,
        TimeProvider timeProvider
    )
    {
        _logger = logger;
        _options = options;
        _timeProvider = timeProvider;
    }

    public ErrorResult<UserAuthError> ValidateEmail(string email)
    {
        email = email.ToLowerInvariant().Trim();

        if (!IsLessOrEqualThanMaximumLength(email, _options.Value.Email.MaximumEmailLength))
        {
            return ErrorResult<UserAuthError>.Failure(UserAuthError.EmailIsTooLong);
        }

        //local part should be <= 64
        var localPart = email.Split("@").FirstOrDefault();
        if (string.IsNullOrWhiteSpace(localPart) || localPart.Length > 64)
        {
            _logger.LogInformation("{EmailLocalPart} is invalid email local part.", localPart);
            return ErrorResult<UserAuthError>.Failure(UserAuthError.InvalidEmail);
        }

        return ErrorResult<UserAuthError>.Success();
    }

    public ErrorResult<UserAuthError> ValidateUsername(string username)
    {
        username = username.ToLowerInvariant().Trim();

        if (!IsGreaterOrEqualMinimumLength(username, _options.Value.Username.MinimumUsernameLength))
        {
            return ErrorResult<UserAuthError>.Failure(UserAuthError.UsernameIsTooShort);
        }

        if (!IsLessOrEqualThanMaximumLength(username, _options.Value.Username.MaximumUsernameLength))
        {
            return ErrorResult<UserAuthError>.Failure(UserAuthError.UsernameIsTooLong);
        }

        if (!Regex.IsMatch(username, _options.Value.Username.AllowedCharactersRegex))
        {
            _logger.LogInformation("username doesn't match allowed characters.");
            return ErrorResult<UserAuthError>.Failure(UserAuthError.InvalidUsername);
        }

        return ErrorResult<UserAuthError>.Success();
    }

    public ErrorResult<UserAuthError> ValidatePassword(string password)
    {
        if (!IsGreaterOrEqualMinimumLength(password, _options.Value.Password.MinimumPasswordLength))
        {
            return ErrorResult<UserAuthError>.Failure(UserAuthError.PasswordIsTooShort);
        }

        if (!IsLessOrEqualThanMaximumLength(password, _options.Value.Password.MaximumPasswordLength))
        {
            return ErrorResult<UserAuthError>.Failure(UserAuthError.PasswordIsTooLong);
        }

        return ErrorResult<UserAuthError>.Success();
    }

    public ErrorResult<UserAuthError> ValidateDateOfBirth(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

        if (dateOfBirth > today)
        {
            _logger.LogInformation("date of birth is in the future.");
            return ErrorResult<UserAuthError>.Failure(UserAuthError.InvalidDateOfBirth);
        }

        var minimumAge = _options.Value.DateOfBirth.MinimumAge;
        var cutoffDate = today.AddYears(-minimumAge);

        if (dateOfBirth > cutoffDate)
        {
            _logger.LogInformation("date of birth does not meet minimum age restrictions.");
            return ErrorResult<UserAuthError>.Failure(UserAuthError.UnderMinimumAge);
        }

        var maximumAge = _options.Value.DateOfBirth.MaximumAge;

        var oldestAllowedDate = today.AddYears(-maximumAge);
        if (dateOfBirth < oldestAllowedDate) 
        {
            _logger.LogInformation("date of birth exceeds maximum age restrictions.");
            return ErrorResult<UserAuthError>.Failure(UserAuthError.ExceedsMaximumAge);
        }


        return ErrorResult<UserAuthError>.Success();
    }

    private bool IsGreaterOrEqualMinimumLength(string value, int minimumLength, [CallerArgumentExpression(nameof(value))] string? valueName = null)
    {
        if (value.Length >= minimumLength)
        {
            return true;
        }
        else
        {
            _logger.LogInformation("{ValidatedValueName} length {ActualValueLength} is less than {MinimumValueLength} required.", valueName, value.Length, minimumLength);
            return false;
        }
    }

    private bool IsLessOrEqualThanMaximumLength(string value, int maximumLength, [CallerArgumentExpression(nameof(value))] string? valueName = null)
    {
        if (value.Length <= maximumLength)
        {
            return true;
        }
        else
        {
            _logger.LogInformation("{ValidatedValueName} length {ActualValueLength} is more than {MaximumValueLength} allowed.", valueName, value.Length, maximumLength);
            return false;
        }
    }
}
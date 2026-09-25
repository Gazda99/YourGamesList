using System.Runtime.CompilerServices;
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
    /// Validates the string value of <paramref name="password"/>. It does not check the DB availability!
    /// </summary>
    ErrorResult<UserAuthError> ValidatePassword(string password);
}

public class UserValuesValidator : IUserValuesValidator
{
    private readonly ILogger<UserValuesValidator> _logger;
    private readonly IOptions<UserValuesValidatorOptions> _options;

    public UserValuesValidator(
        ILogger<UserValuesValidator> logger,
        IOptions<UserValuesValidatorOptions> options)
    {
        _logger = logger;
        _options = options;
    }

    public ErrorResult<UserAuthError> ValidateEmail(string email)
    {
        if (!IsLessOrEqualThanMaximumLength(email, _options.Value.Email.MaximumEmailLength))
        {
            return ErrorResult<UserAuthError>.Failure(UserAuthError.EmailIsTooLong);
        }
        
        return ErrorResult<UserAuthError>.Success();
    }

    public ErrorResult<UserAuthError> ValidateUsername(string username)
    {
        if (!IsGreaterOrEqualMinimumLength(username, _options.Value.Username.MinimumUsernameLength))
        {
            return ErrorResult<UserAuthError>.Failure(UserAuthError.UsernameIsTooShort);
        }

        if (!IsLessOrEqualThanMaximumLength(username, _options.Value.Username.MaximumUsernameLength))
        {
            return ErrorResult<UserAuthError>.Failure(UserAuthError.UsernameIsTooLong);
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
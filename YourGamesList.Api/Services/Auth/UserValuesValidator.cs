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
        return ErrorResult<UserAuthError>.Success();
    }

    public ErrorResult<UserAuthError> ValidateUsername(string username)
    {
        return ErrorResult<UserAuthError>.Success();
    }

    public ErrorResult<UserAuthError> ValidatePassword(string password)
    {
        if (!IsGreaterOrEqualMinimumLength(password, _options.Value.MinimumPasswordLength))
        {
            return ErrorResult<UserAuthError>.Failure(UserAuthError.PasswordIsTooShort);
        }

        if (!IsLessOrEqualThanMaximumLength(password, _options.Value.MaximumPasswordLength))
        {
            return ErrorResult<UserAuthError>.Failure(UserAuthError.PasswordIsTooLong);
        }

        return ErrorResult<UserAuthError>.Success();
    }

    private bool IsGreaterOrEqualMinimumLength(string password, int minimumLength)
    {
        if (password.Length >= minimumLength)
        {
            return true;
        }
        else
        {
            _logger.LogInformation("Password length {ActualPasswordLength} is less than {MinimumPasswordLength} required.", password.Length, minimumLength);
            return false;
        }
    }

    private bool IsLessOrEqualThanMaximumLength(string password, int maximumLength)
    {
        if (password.Length <= maximumLength)
        {
            return true;
        }
        else
        {
            _logger.LogInformation("Password length {ActualPasswordLength} is more than {MaximumPasswordLength} allowed.", password.Length, maximumLength);
            return false;
        }
    }
}
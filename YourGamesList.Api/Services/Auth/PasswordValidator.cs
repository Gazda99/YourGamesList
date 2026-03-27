using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourGamesList.Api.Services.Auth.Model;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Common;

namespace YourGamesList.Api.Services.Auth;

public interface IPasswordValidator
{
    ErrorResult<UserAuthError> ValidatePassword(string password);
}

public class PasswordValidator : IPasswordValidator
{
    private readonly ILogger<PasswordValidator> _logger;
    private readonly IOptions<PasswordValidatorOptions> _options;

    public PasswordValidator(
        ILogger<PasswordValidator> logger,
        IOptions<PasswordValidatorOptions> options)
    {
        _logger = logger;
        _options = options;
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

        return ErrorResult<UserAuthError>.Clear();
    }

    private bool IsGreaterOrEqualMinimumLength(string password, int minimumLength)
    {
        if (password.Length >= minimumLength)
        {
            return true;
        }
        else
        {
            _logger.LogInformation($"Password length '{password.Length}' is less than '{minimumLength}' required.");
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
            _logger.LogInformation($"Password length '{password.Length}' is more than '{maximumLength}' allowed.");
            return false;
        }
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.Auth.Model;
using YourGamesList.Common;

namespace YourGamesList.Api.Services.Auth;

public interface IUserManagerService
{
    Task<CombinedResult<Guid, UserAuthError>> Register(UserRegisterDto userRegisterDto);
    Task Login(UserLoginDto userLoginDto);
    Task Delete(UserDeleteDto userDeleteDto);
}

public class UserManagerService : IUserManagerService
{
    private readonly ILogger<UserManagerService> _logger;
    private readonly IUserValuesValidator _userValuesValidator;

    public UserManagerService(ILogger<UserManagerService> logger, IUserValuesValidator userValuesValidator)
    {
        _logger = logger;
        _userValuesValidator = userValuesValidator;
    }

    public async Task Login(UserLoginDto userLoginDto)
    {
        throw new System.NotImplementedException();
    }

    public async Task<CombinedResult<Guid, UserAuthError>> Register(UserRegisterDto userRegisterDto)
    {
        var validateEmail = _userValuesValidator.ValidateEmail(userRegisterDto.Email);
        if (validateEmail.IsFailure)
        {
            _logger.LogInformation("Cannot register user due to email being invalid. Error: {UserRegisterError}", validateEmail.Error.ToString());
            return CombinedResult<Guid, UserAuthError>.Failure(validateEmail.Error);
        }

        var validateUsername = _userValuesValidator.ValidateUsername(userRegisterDto.Username);
        if (validateUsername.IsFailure)
        {
            _logger.LogInformation("Cannot register user due to username being invalid. Error: {UserRegisterError}", validateUsername.Error.ToString());
            return CombinedResult<Guid, UserAuthError>.Failure(validateUsername.Error);
        }

        var validatePasswordResult = _userValuesValidator.ValidatePassword(userRegisterDto.Password);
        if (validatePasswordResult.IsFailure)
        {
            _logger.LogInformation("Cannot register user due to password being invalid. Error: {UserRegisterError}", validatePasswordResult.Error.ToString());
            return CombinedResult<Guid, UserAuthError>.Failure(validatePasswordResult.Error);
        }

        return CombinedResult<Guid, UserAuthError>.Success(Guid.NewGuid());
    }

    public async Task Delete(UserDeleteDto userDeleteDto)
    {
        throw new System.NotImplementedException();
    }
}
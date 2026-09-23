using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Model;

namespace YourGamesList.Api.ModelBinders;

public class UserInformationTokenModelBinder : IModelBinder
{
    private readonly ILogger<UserInformationTokenModelBinder> _logger;

    public UserInformationTokenModelBinder(ILogger<UserInformationTokenModelBinder> logger)
    {
        _logger = logger;
    }


    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        if (bindingContext.ModelType != typeof(UserInformationToken))
        {
            return Task.CompletedTask;
        }

        var userTokenResult = UserInformationToken.FromClaimsPrincipal(bindingContext.HttpContext.User);
        if (userTokenResult.IsFailure)
        {
            _logger.LogWarning(userTokenResult.Error.Exception, "User token cannot be parsed. Error: {UserTokenCreationErrorMessage}", userTokenResult.Error.Message);
            throw new UnauthorizedAccessException(userTokenResult.Error.Message);
        }

        bindingContext.Result = ModelBindingResult.Success(userTokenResult.Value);

        return Task.CompletedTask;
    }
}
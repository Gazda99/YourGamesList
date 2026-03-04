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
        if (bindingContext == null)
        {
            throw new ModelBindingException()
            {
                ModelName = "N/A",
                ModelType = null,
                ErrorDescription = $"{nameof(bindingContext)} is null"
            };
        }

        if (bindingContext.ModelType != typeof(UserInformationToken))
        {
            _logger.LogWarning(
                $"Attempted to bind model of type '{bindingContext.ModelType.Name}' with '{nameof(UserInformationTokenModelBinder)}'. This binder is only for '{nameof(UserInformationToken)}'.");
            return Task.CompletedTask;
        }

        if (bindingContext.HttpContext.Items.TryGetValue(nameof(UserInformationToken), out var item) && item is UserInformationToken cachedUserInformation)
        {
            bindingContext.Result = ModelBindingResult.Success(cachedUserInformation);
            _logger.LogInformation($"Successfully bound '{nameof(UserInformationToken)}' in '{bindingContext.OriginalModelName}'.");
            return Task.CompletedTask;
        }

        const string errorMessage = "JwtUserInformation is missing. Authentication Middleware failure or token parsing error.";
        _logger.LogError(errorMessage);
        bindingContext.Result = ModelBindingResult.Failed();
        throw new ModelBindingException
        {
            ModelName = bindingContext.OriginalModelName,
            ModelType = bindingContext.ModelType,
            ErrorDescription = errorMessage
        };
    }
}
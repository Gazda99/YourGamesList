using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Model;

namespace YourGamesList.Api.ModelBinders;

public class JwtUserInformationModelBinder : IModelBinder
{
    private readonly ILogger<JwtUserInformationModelBinder> _logger;

    public JwtUserInformationModelBinder(ILogger<JwtUserInformationModelBinder> logger)
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

        if (bindingContext.ModelType != typeof(JwtUserInformation))
        {
            _logger.LogWarning(
                $"Attempted to bind model of type '{bindingContext.ModelType.Name}' with '{nameof(JwtUserInformationModelBinder)}'. This binder is only for '{nameof(JwtUserInformation)}'.");
            return Task.CompletedTask;
        }

        if (bindingContext.HttpContext.Items.TryGetValue(nameof(JwtUserInformation), out var item) && item is JwtUserInformation cachedUserInformation)
        {
            bindingContext.Result = ModelBindingResult.Success(cachedUserInformation);
            _logger.LogInformation($"Successfully bound '{nameof(JwtUserInformation)}' in '{bindingContext.OriginalModelName}'.");
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
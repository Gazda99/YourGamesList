using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using YourGamesList.Api.Model;
using YourGamesList.Api.Services.Auth;

namespace YourGamesList.Api.ModelBinders;

//TODO: unit tests
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
            _logger.LogWarning($"Attempted to bind model of type '{bindingContext.ModelType.Name}' with '{nameof(JwtUserInformationModelBinder)}'. This binder is only for '{nameof(JwtUserInformation)}'.");
            return Task.CompletedTask;
        }

        var authorizationHeader = bindingContext.HttpContext.Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            _logger.LogInformation("Authorization header missing or not in 'Bearer' format.");
            bindingContext.Result = ModelBindingResult.Failed();
            throw new ModelBindingException()
            {
                ModelName = bindingContext.OriginalModelName,
                ModelType = bindingContext.ModelType,
                ErrorDescription = "Authorization header missing or not in 'Bearer' format."
            };
        }

        var token = authorizationHeader.Substring("Bearer ".Length).Trim();

        var handler = new JsonWebTokenHandler();

        if (!handler.CanReadToken(token))
        {
            _logger.LogWarning("Cannot read JWT.");
            bindingContext.Result = ModelBindingResult.Failed();
            throw new ModelBindingException()
            {
                ModelName = bindingContext.OriginalModelName,
                ModelType = bindingContext.ModelType,
                ErrorDescription = "Cannot read JWT from authorization header."
            };
        }

        var jwtToken = handler.ReadJsonWebToken(token);

        var userInformation = new JwtUserInformation
        {
            Username = ReadClaimOrThrow(bindingContext, jwtToken.Claims, JwtRegisteredClaimNames.Sub),
            UserId = ReadClaimOrThrow(bindingContext, jwtToken.Claims, JwtCustomClaimNames.UserId)
        };

        _logger.LogInformation($"Successfully bound '{nameof(JwtUserInformation)}' in '{bindingContext.OriginalModelName}'.");
        bindingContext.Result = ModelBindingResult.Success(userInformation);
        return Task.CompletedTask;
    }

    private static string ReadClaimOrThrow(ModelBindingContext bindingContext, IEnumerable<Claim> claims, string claimType)
    {
        return claims.FirstOrDefault(c => c.Type == claimType)?.Value ?? throw new ModelBindingException()
        {
            ModelName = bindingContext.OriginalModelName,
            ModelType = bindingContext.ModelType,
            ErrorDescription = "Cannot read JWT from authorization header."
        };
    }
}
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

public class JwtUserInformationModelBinder : IModelBinder
{
    private readonly ILogger<JwtUserInformationModelBinder> _logger;
    private readonly ITokenParser _tokenParser;

    public JwtUserInformationModelBinder(ILogger<JwtUserInformationModelBinder> logger, ITokenParser tokenParser)
    {
        _logger = logger;
        _tokenParser = tokenParser;
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
            const string errorMessage = "Authorization header missing or not in 'Bearer' format.";
            _logger.LogInformation(errorMessage);
            bindingContext.Result = ModelBindingResult.Failed();
            throw new ModelBindingException()
            {
                ModelName = bindingContext.OriginalModelName,
                ModelType = bindingContext.ModelType,
                ErrorDescription = errorMessage
            };
        }

        var token = authorizationHeader.Substring("Bearer ".Length).Trim();

        if (!_tokenParser.CanReadToken(token))
        {
            const string errorMessage = "Cannot read JWT from authorization header.";
            _logger.LogInformation(errorMessage);
            bindingContext.Result = ModelBindingResult.Failed();
            throw new ModelBindingException()
            {
                ModelName = bindingContext.OriginalModelName,
                ModelType = bindingContext.ModelType,
                ErrorDescription = errorMessage
            };
        }

        var jwtToken = _tokenParser.ReadJsonWebToken(token);

        var userInformation = new JwtUserInformation
        {
            Username = ReadClaimOrThrow(bindingContext, jwtToken.Claims, JwtRegisteredClaimNames.Sub),
            UserId = ReadClaimOrThrow(bindingContext, jwtToken.Claims, JwtCustomClaimNames.UserId)
        };

        _logger.LogInformation($"Successfully bound '{nameof(JwtUserInformation)}' in '{bindingContext.OriginalModelName}'.");
        bindingContext.Result = ModelBindingResult.Success(userInformation);
        return Task.CompletedTask;
    }

    private string ReadClaimOrThrow(ModelBindingContext bindingContext, IEnumerable<Claim> claims, string claimType)
    {
        var claim = claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        if (claim == null)
        {
            var errorMessage = $"Cannot read JWT '{claimType}' claim.";
            _logger.LogInformation(errorMessage);
            throw new ModelBindingException()
            {
                ModelName = bindingContext.OriginalModelName,
                ModelType = bindingContext.ModelType,
                ErrorDescription = errorMessage
            };
        }

        return claim;
    }
}
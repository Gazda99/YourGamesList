using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Common.Http;

namespace YourGamesList.Api.Services.Auth.Filters;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ApiKeyAuthFilterAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _requiredApiKeyName;
    private readonly ILogger<ApiKeyAuthFilterAttribute> _logger;
    private readonly IOptions<ApiKeysOptions> _apiKeysOptions;

    public ApiKeyAuthFilterAttribute(string requiredApiKeyName, ILogger<ApiKeyAuthFilterAttribute> logger, IOptions<ApiKeysOptions> apiKeysOptions)
    {
        _requiredApiKeyName = requiredApiKeyName;
        _logger = logger;
        _apiKeysOptions = apiKeysOptions;

        if (string.IsNullOrEmpty(requiredApiKeyName))
        {
            throw new ArgumentNullException(nameof(requiredApiKeyName), "Required api key is empty/null");
        }

        if (!_apiKeysOptions.Value.Keys.ContainsKey(_requiredApiKeyName))
        {
            throw new InvalidOperationException("Required api key is missing from configuration");
        }
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        _logger.LogDebug($"Looking for API key '{_requiredApiKeyName}'");

        if (!context.HttpContext.Request.Headers.TryGetValue(YglHttpHeaders.ApiKeyHeader, out var apiKey))
        {
            const string apiKeyMissingMessage = "Missing Api Key";
            _logger.LogWarning(apiKeyMissingMessage);
            var response = context.HttpContext.Response;
            response.StatusCode = (int) HttpStatusCode.Unauthorized;
            await response.WriteAsync(apiKeyMissingMessage);

            return;
        }

        var expectedApiKey = _apiKeysOptions.Value.Keys[_requiredApiKeyName];
        if (apiKey != expectedApiKey)
        {
            const string apiKeyWrongMessage = "Wrong Api Key";
            _logger.LogWarning(apiKeyWrongMessage);
            var response = context.HttpContext.Response;
            response.StatusCode = (int) HttpStatusCode.Unauthorized;
            await response.WriteAsync(apiKeyWrongMessage);

            return;
        }

        _logger.LogDebug("Api key correct");

        await next();
    }
}
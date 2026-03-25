using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using YourGamesList.Api.Model;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Common.Logging;
using YourGamesList.Database.Entities;

namespace YourGamesList.Api.Middlewares;

public class JwtUserInformationMiddleware
{
    private static readonly Type[] AuthorizeDataTypesThatRequireAuth =
    [
        typeof(AuthorizeAttribute)
    ];

    private readonly RequestDelegate _next;
    private readonly ILogger<JwtUserInformationMiddleware> _logger;
    private readonly ITokenParser _tokenParser;

    public JwtUserInformationMiddleware(RequestDelegate next, ILogger<JwtUserInformationMiddleware> logger, ITokenParser tokenParser)
    {
        _next = next;
        _logger = logger;
        _tokenParser = tokenParser;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var metadata = endpoint?.Metadata.GetMetadata<IAuthorizeData>();
        if (metadata == null || !AuthorizeDataTypesThatRequireAuth.Contains(metadata.GetType()))
        {
            await _next(context);
            return;
        }

        const string bearerPrefix = "Bearer ";
        var authorizationHeader = context.Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith(bearerPrefix))
        {
            _logger.LogInformation("Authorization header missing or not in 'Bearer' format.");
            await ReturnUnauthorized(context.Response);
            return;
        }

        var token = authorizationHeader.Substring(bearerPrefix.Length).Trim();

        if (!_tokenParser.CanReadToken(token))
        {
            _logger.LogInformation("Cannot read JWT from authorization header.");
            await ReturnUnauthorized(context.Response);
            return;
        }

        var jwtToken = _tokenParser.ReadJsonWebToken(token);

        UserInformationToken userInformationToken;
        try
        {
            userInformationToken = UserInformationToken.FromJwt(jwtToken);
        }
        catch (BaseTokenCreationException ex)
        {
            _logger.LogError(ex, "Cannot read JWT from authorization header.");
            await ReturnUnauthorized(context.Response);
            return;
        }

        context.Items[nameof(UserInformationToken)] = userInformationToken;

        using (_logger.BeginScope(new Dictionary<string, object> { [LogProperties.UserId] = userInformationToken.UserId.ToString() }))
        {
            await _next(context);
        }
    }

    private static async Task ReturnUnauthorized(HttpResponse response)
    {
        response.StatusCode = (int) HttpStatusCode.Unauthorized;
        await response.WriteAsync(string.Empty);
    }
}
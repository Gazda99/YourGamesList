using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using YourGamesList.Common.Http;

namespace YourGamesList.Api.AppBuilders;

public static partial class AppBuilder
{
    private static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            //API KEY
            c.AddSecurityDefinition("YglApiKey", new OpenApiSecurityScheme
            {
                Description = $"API Key authorization. Enter the key in the '{HttpHeaders.ApiKeyHeader}' header.",
                Type = SecuritySchemeType.ApiKey,
                Name = HttpHeaders.ApiKeyHeader,
                In = ParameterLocation.Header,
                Scheme = "YglApiKey"
            });

            var apiKeySchema = new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "YglApiKey"
                },
                In = ParameterLocation.Header
            };
            var apiKeyRequirements = new OpenApiSecurityRequirement
            {
                { apiKeySchema, [] } // No specific scopes are required
            };
            c.AddSecurityRequirement(apiKeyRequirements);


            //Bearer Auth
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = HttpHeaders.Authorization,
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer"
            });

            var bearerTokenSchema = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            var bearerTokenRequirements = new OpenApiSecurityRequirement
            {
                { bearerTokenSchema, [] }
            };

            c.AddSecurityRequirement(bearerTokenRequirements);
        });

        services.AddEndpointsApiExplorer();

        return services;
    }
}
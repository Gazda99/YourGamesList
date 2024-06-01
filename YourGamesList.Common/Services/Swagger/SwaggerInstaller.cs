using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace YourGamesList.Common.Services.Swagger;

[ExcludeFromCodeCoverage]
public static class SwaggerInstaller
{
    public static IServiceCollection AddSwaggerDefinitions(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Your Games List API",
                Version = "v1.0",
                Description = "Your Games List API"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Bearer auth header.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
        });
        return services;
    }
}
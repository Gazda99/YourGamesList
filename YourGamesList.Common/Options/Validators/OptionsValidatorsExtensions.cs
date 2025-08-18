using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace YourGamesList.Common.Options.Validators;

[ExcludeFromCodeCoverage]
public static class OptionsValidatorsExtensions
{
    public static IServiceCollection AddOptionsWithFluentValidation<TOptions, TValidator>(
        this IServiceCollection services, string configurationSection) where TOptions : class
        where TValidator : class, IValidator<TOptions>
    {
        services.TryAddScoped<IValidator<TOptions>, TValidator>();
        services.TryAddScoped<TValidator>();


        if (services.All(sd => sd.ServiceType != typeof(IConfigureOptions<TOptions>)))
        {
            services.AddOptions<TOptions>()
                .BindConfiguration(configurationSection)
                .ValidateFluentValidation()
                .ValidateOnStart();
        }

        return services;
    }

    private static OptionsBuilder<TOptions> ValidateFluentValidation<TOptions>(this OptionsBuilder<TOptions> builder) where TOptions : class
    {
        builder.Services.TryAddSingleton<IValidateOptions<TOptions>>(serviceProvider =>
            new FluentValidateOptions<TOptions>(serviceProvider, builder.Name));
        return builder;
    }
}
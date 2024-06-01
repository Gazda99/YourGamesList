using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace YourGamesList.Common.Options.Validators;

[ExcludeFromCodeCoverage]
public static class OptionsValidatorsExtensions
{
    public static IServiceCollection AddOptionsWithFluentValidation<TOptions>(this IServiceCollection services,
        string configurationSection) where TOptions : class
    {
        services.AddOptions<TOptions>().BindConfiguration(configurationSection)
            .ValidateFluentValidation().ValidateOnStart();

        return services;
    }

    private static OptionsBuilder<TOptions> ValidateFluentValidation<TOptions>(this OptionsBuilder<TOptions> builder)
        where TOptions : class
    {
        builder.Services.AddSingleton<IValidateOptions<TOptions>>(serviceProvider =>
            new FluentValidateOptions<TOptions>(serviceProvider, builder.Name));

        return builder;
    }
}
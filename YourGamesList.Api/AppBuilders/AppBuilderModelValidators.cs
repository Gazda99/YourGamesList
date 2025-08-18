using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using YourGamesList.Api.ControllerModelValidators;
using YourGamesList.Api.Model.Requests.Auth;

namespace YourGamesList.Api.AppBuilders;

public static partial class AppBuilder
{
    private static IServiceCollection AddRequestModelValidators(this IServiceCollection services)
    {
        //Essentials
        services
            .AddScoped<IBaseRequestValidator, BaseRequestValidator>()
            .AddScoped<IValidationFailedResultFactory, ValidationFailedResultFactory>()
            ;

        //Auth controller validators
        services
            .AddScoped<IValidator<UserRegisterRequest>, UserRegisterRequestValidator>()
            .AddScoped<IValidator<UserLoginRequest>, UserLoginRequestValidator>()
            .AddScoped<IValidator<UserDeleteRequest>, UserDeleteRequestValidator>()
            ;

        return services;
    }
}
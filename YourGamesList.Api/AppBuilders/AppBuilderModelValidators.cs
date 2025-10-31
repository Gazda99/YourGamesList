using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using YourGamesList.Api.ControllerModelValidators;
using YourGamesList.Api.Model.Requests.Auth;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Model.Requests.SearchIgdbGames;

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

        //Search Games controller validators
        services
            .AddScoped<IValidator<SearchIgdbGameByNameRequest>, SearchGameByNameRequestValidator>()
            .AddScoped<IValidator<SearchIgdbGamesByIdsRequest>, SearchGamesByIdsRequestValidator>()
            ;

        //Lists controller validators
        services
            .AddScoped<IValidator<CreateListRequest>, CreateListRequestValidator>()
            .AddScoped<IValidator<GetListsRequest>, GetListsRequestValidator>()
            .AddScoped<IValidator<UpdateListRequest>, UpdateListRequestValidator>()
            .AddScoped<IValidator<DeleteListRequest>, DeleteListRequestValidator>()
            ;

        return services;
    }
}
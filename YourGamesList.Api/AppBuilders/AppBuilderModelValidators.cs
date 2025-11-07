using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using YourGamesList.Api.ControllerModelValidators;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Auth;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Model.Requests.SearchIgdbGames;
using YourGamesList.Api.Model.Requests.SearchYglGames;

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

        //Common
        services
            .AddScoped<IValidator<JwtUserInformation>, JwtUserInformationValidator>()
            ;

        //Auth controller validators
        services
            .AddScoped<IValidator<UserRegisterRequest>, UserRegisterRequestValidator>()
            .AddScoped<IValidator<UserLoginRequest>, UserLoginRequestValidator>()
            .AddScoped<IValidator<UserDeleteRequest>, UserDeleteRequestValidator>()
            ;

        //Search IGDB Games controller validators
        services
            .AddScoped<IValidator<SearchIgdbGameByNameRequest>, SearchGameByNameRequestValidator>()
            .AddScoped<IValidator<SearchIgdbGamesByIdsRequest>, SearchGamesByIdsRequestValidator>()
            ;

        //Search YGL Games controller validators
        services
            .AddScoped<IValidator<SearchYglGamesRequest>, SearchYglGamesRequestValidator>()
            ;

        //Lists controller validators
        services
            //list
            .AddScoped<IValidator<CreateListRequest>, CreateListRequestValidator>()
            .AddScoped<IValidator<SearchListsRequest>, SearchListsRequestValidator>()
            .AddScoped<IValidator<GetSelfListsRequest>, GetSelfListsRequestValidator>()
            .AddScoped<IValidator<UpdateListRequest>, UpdateListRequestValidator>()
            .AddScoped<IValidator<DeleteListRequest>, DeleteListRequestValidator>()
            //list entries
            .AddScoped<IValidator<AddEntriesToListRequest>, AddEntriesToListRequestValidator>()
            .AddScoped<IValidator<DeleteListEntriesRequest>, DeleteListEntriesRequestValidator>()
            .AddScoped<IValidator<UpdateListEntriesRequest>, UpdateListEntriesRequestValidator>()
            ;

        return services;
    }
}
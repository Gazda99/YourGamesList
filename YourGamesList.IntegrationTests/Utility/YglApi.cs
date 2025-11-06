using System.Threading.Tasks;
using Refit;
using YourGamesList.IntegrationTests.Requests.Auth;

namespace YourGamesList.IntegrationTests.Utility;

public class YglApi
{
    public IYglAuthApi Auth { get; private set; }
    public IListsApi Lists { get; private set; }
    public ISearchIgdbGamesApi SearchIgdbGames { get; private set; }
    public IScraperApi Scraper { get; private set; }

    public YglApi()
    {
        var yglBaseUrl = Configuration.YourGamesListBaseUrl;

        Auth = RestService.For<IYglAuthApi>(yglBaseUrl);
        //Lists = RestService.For<IListsApi>(yglBaseUrl);
      //  SearchIgdbGames = RestService.For<ISearchIgdbGamesApi>(yglBaseUrl);
       // Scraper = RestService.For<IScraperApi>(yglBaseUrl);
    }
}

public interface IYglAuthApi
{
    [Post("/auth/register")]
    Task<IApiResponse> Register(
        [Body] UserRegisterRequest registerRequest
    );

    [Post("/auth/login")]
    Task<IApiResponse<string>>  Login(
        [Body] UserLoginRequest loginRequest
    );

    [Delete("/auth/delete")]
    Task<IApiResponse> Delete(
        [Body] UserDeleteRequest deleteRequest
    );
}

public interface IListsApi
{
}

public interface ISearchIgdbGamesApi
{
}

public interface IScraperApi
{
}
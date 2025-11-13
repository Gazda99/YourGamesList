using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YourGamesList.Common;
using YourGamesList.Common.Refit;
using YourGamesList.Web.Page.Services.Ygl.Model;
using YourGamesList.Web.Page.Services.Ygl.Model.Requests;

namespace YourGamesList.Web.Page.Services.Ygl;

public interface IYglAuthClient
{
    Task<ErrorResult<YglAuthAuthClientError>> Register(string username, string password);
    Task<CombinedResult<string, YglAuthAuthClientError>> Login(string username, string password);
    Task<ErrorResult<YglAuthAuthClientError>> Delete(string username, string password);
}

//TODO: unit tests
public class YglAuthAuthClient : IYglAuthClient
{
    private readonly ILogger<YglAuthAuthClient> _logger;
    private readonly IYglAuthApi _authApi;

    public YglAuthAuthClient(ILogger<YglAuthAuthClient> logger, IYglAuthApi authApi)
    {
        _logger = logger;
        _authApi = authApi;
    }

    public async Task<ErrorResult<YglAuthAuthClientError>> Register(string username, string password)
    {
        var request = new UserRegisterRequest()
        {
            Username = username,
            Password = password
        };

        _logger.LogInformation($"Sending register request for user '{username}'.");
        var callResult = await _authApi.TryRefit(() => _authApi.Register(request), _logger);

        if (callResult.IsFailure)
        {
            return ErrorResult<YglAuthAuthClientError>.Failure(YglAuthAuthClientError.General);
        }

        var res = callResult.Value;
        if (res.StatusCode == HttpStatusCode.OK)
        {
            return ErrorResult<YglAuthAuthClientError>.Clear();
        }
        else if (res.StatusCode == HttpStatusCode.Conflict)
        {
            return ErrorResult<YglAuthAuthClientError>.Failure(YglAuthAuthClientError.RegisterUserAlreadyExists);
        }
        else if (res.StatusCode == HttpStatusCode.BadRequest)
        {
            if (res.Error?.Content is "PasswordIsTooShort" or "PasswordIsTooLong")
            {
                return ErrorResult<YglAuthAuthClientError>.Failure(YglAuthAuthClientError.RegisterWeakPassword);
            }
            else
            {
                return ErrorResult<YglAuthAuthClientError>.Failure(YglAuthAuthClientError.General);
            }
        }
        else
        {
            return ErrorResult<YglAuthAuthClientError>.Failure(YglAuthAuthClientError.General);
        }
    }

    public async Task<CombinedResult<string, YglAuthAuthClientError>> Login(string username, string password)
    {
        var request = new UserLoginRequest()
        {
            Username = username,
            Password = password
        };

        _logger.LogInformation($"Sending login request for user '{username}'.");
        var callResult = await _authApi.TryRefit(() => _authApi.Login(request), _logger);

        if (callResult.IsFailure)
        {
            return CombinedResult<string, YglAuthAuthClientError>.Failure(YglAuthAuthClientError.General);
        }

        var res = callResult.Value;

        if (res.StatusCode == HttpStatusCode.OK)
        {
            _logger.LogInformation("Received successful response from Ygl Auth Api for login request.");
            return CombinedResult<string, YglAuthAuthClientError>.Success(res.Content!.Token);
        }
        else if (res.StatusCode == HttpStatusCode.Unauthorized)
        {
            return CombinedResult<string, YglAuthAuthClientError>.Failure(YglAuthAuthClientError.LoginUnauthorized);
        }
        else if (res.StatusCode == HttpStatusCode.NotFound)
        {
            return CombinedResult<string, YglAuthAuthClientError>.Failure(YglAuthAuthClientError.LoginUserNotFound);
        }
        else
        {
            return CombinedResult<string, YglAuthAuthClientError>.Failure(YglAuthAuthClientError.General);
        }
    }

    public async Task<ErrorResult<YglAuthAuthClientError>> Delete(string username, string password)
    {
        var request = new UserDeleteRequest()
        {
            Username = username,
            Password = password
        };

        _logger.LogInformation($"Sending delete request for user '{username}'.");
        var callResult = await _authApi.TryRefit(() => _authApi.Delete(request), _logger);

        if (callResult.IsFailure)
        {
            return ErrorResult<YglAuthAuthClientError>.Failure(YglAuthAuthClientError.General);
        }

        var res = callResult.Value;
        if (res.StatusCode == HttpStatusCode.NoContent)
        {
            return ErrorResult<YglAuthAuthClientError>.Clear();
        }
        else if (res.StatusCode == HttpStatusCode.NotFound)
        {
            return ErrorResult<YglAuthAuthClientError>.Failure(YglAuthAuthClientError.DeleteUserNotFound);
        }
        else
        {
            return ErrorResult<YglAuthAuthClientError>.Failure(YglAuthAuthClientError.General);
        }
    }
}
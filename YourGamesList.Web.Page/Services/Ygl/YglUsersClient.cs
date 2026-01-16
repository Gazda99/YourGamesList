using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YourGamesList.Common;
using YourGamesList.Common.Refit;
using YourGamesList.Contracts.Dto;
using YourGamesList.Contracts.Requests.Users;
using YourGamesList.Web.Page.Services.Ygl.Model;

namespace YourGamesList.Web.Page.Services.Ygl;

public interface IYglUsersClient
{
    Task<CombinedResult<UserDto, YglUserClientError>> GetSelfUser(string userToken);
    Task<CombinedResult<Guid, YglUserClientError>> UpdateUser(string userToken, UserUpdateRequestBody body);
    Task<CombinedResult<string[], YglUserClientError>> GetCountries(string userToken);
}

//TODO: unit tests
//TODO: update user methods
public class YglUsersClient : IYglUsersClient
{
    private readonly ILogger<YglUsersClient> _logger;
    private readonly IYglApi _yglApi;

    public YglUsersClient(ILogger<YglUsersClient> logger, IYglApi yglApi)
    {
        _logger = logger;
        _yglApi = yglApi;
    }

    public async Task<CombinedResult<UserDto, YglUserClientError>> GetSelfUser(string userToken)
    {
        _logger.LogInformation("Sending request to get self user.");
        var callResult = await _yglApi.Users.TryRefit(() => _yglApi.Users.GetSelfUser(userToken), _logger);

        if (callResult.IsFailure)
        {
            return CombinedResult<UserDto, YglUserClientError>.Failure(YglUserClientError.General);
        }

        var res = callResult.Value;
        if (res.StatusCode == HttpStatusCode.OK)
        {
            return CombinedResult<UserDto, YglUserClientError>.Success(res.Content!);
        }
        else if (res.StatusCode == HttpStatusCode.NotFound)
        {
            return CombinedResult<UserDto, YglUserClientError>.Failure(YglUserClientError.UserNotFound);
        }
        else
        {
            return CombinedResult<UserDto, YglUserClientError>.Failure(YglUserClientError.General);
        }
    }

    public async Task<CombinedResult<Guid, YglUserClientError>> UpdateUser(string userToken, UserUpdateRequestBody body)
    {
        _logger.LogInformation("Sending request to update user.");
        var callResult = await _yglApi.Users.TryRefit(() => _yglApi.Users.UpdateUser(userToken, body), _logger);

        if (callResult.IsFailure)
        {
            return CombinedResult<Guid, YglUserClientError>.Failure(YglUserClientError.General);
        }

        var res = callResult.Value;
        if (res.StatusCode == HttpStatusCode.OK)
        {
            return CombinedResult<Guid, YglUserClientError>.Success(res.Content!);
        }
        else if (res.StatusCode == HttpStatusCode.NotFound)
        {
            return CombinedResult<Guid, YglUserClientError>.Failure(YglUserClientError.UserNotFound);
        }
        else if (res.StatusCode == HttpStatusCode.BadRequest)
        {
            return CombinedResult<Guid, YglUserClientError>.Failure(YglUserClientError.BadRequest);
        }
        else
        {
            return CombinedResult<Guid, YglUserClientError>.Failure(YglUserClientError.General);
        }
    }

    public async Task<CombinedResult<string[], YglUserClientError>> GetCountries(string userToken)
    {
        _logger.LogInformation("Sending request to get list of available countries.");
        var callResult = await _yglApi.Users.TryRefit(() => _yglApi.Users.GetAvailableCountries(userToken), _logger);
        if (callResult.IsFailure)
        {
            return CombinedResult<string[], YglUserClientError>.Failure(YglUserClientError.General);
        }

        var res = callResult.Value;
        if (res.StatusCode == HttpStatusCode.OK)
        {
            return CombinedResult<string[], YglUserClientError>.Success(res.Content!);
        }
        else
        {
            return CombinedResult<string[], YglUserClientError>.Failure(YglUserClientError.General);
        }
    }
}
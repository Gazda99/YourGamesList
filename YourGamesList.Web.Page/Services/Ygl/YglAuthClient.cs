using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YourGamesList.Common;
using YourGamesList.Common.Refit;
using YourGamesList.Web.Page.Services.Ygl.Model.Requests;

namespace YourGamesList.Web.Page.Services.Ygl;

public interface IYglAuthClient
{
    Task<CombinedResult<string, string>> Login(string username, string password);
}

public class YglAuthAuthClient : IYglAuthClient
{
    private readonly ILogger<YglAuthAuthClient> _logger;
    private readonly IYglAuthApi _authApi;

    public YglAuthAuthClient(ILogger<YglAuthAuthClient> logger, IYglAuthApi authApi)
    {
        _logger = logger;
        _authApi = authApi;
    }

    public async Task<CombinedResult<string, string>> Login(string username, string password)
    {
        var request = new LoginRequest()
        {
            Username = username,
            Password = password
        };

        _logger.LogInformation($"Sending login request for user '{username}'.");
        var callResult = await _authApi.TryRefit(() => _authApi.Login(request), _logger);

        if (callResult.IsFailure)
        {
            return CombinedResult<string, string>.Failure("");
        }

        var res = callResult.Value;
        if (res.StatusCode == HttpStatusCode.OK)
        {
            _logger.LogInformation("Successfully obtained response from Ygl Auth Api.");
            return CombinedResult<string, string>.Success(res.Content!.Token);
        }
        else
        {
            return CombinedResult<string, string>.Failure("");
        }
    }
}
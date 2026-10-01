using YourGamesList.Api.Model.Requests.Users;

namespace YourGamesList.Api.Services.Auth.Model;

public class ConfirmEmailParams
{
    public required string Token { get; set; }
}

public static class ConfirmEmailParamsMappingExtensions
{
    public static ConfirmEmailParams ToParams(this ConfirmEmailRequest request)
    {
        return new ConfirmEmailParams
        {
            Token = request.Token
        };
    }
}
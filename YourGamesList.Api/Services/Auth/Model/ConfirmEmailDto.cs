using YourGamesList.Api.Model.Requests.Users;

namespace YourGamesList.Api.Services.Auth.Model;

public class ConfirmEmailDto
{
    public required string Token { get; set; }
}

public static class ConfirmEmailDtoMappingExtensions
{
    public static ConfirmEmailDto ToDto(this ConfirmEmailRequest request)
    {
        return new ConfirmEmailDto
        {
            Token = request.Token
        };
    }
}
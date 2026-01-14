using Microsoft.IdentityModel.JsonWebTokens;

namespace YourGamesList.Api.Services.Auth;

public interface ITokenParser
{
    bool CanReadToken(string token);
    JsonWebToken ReadJsonWebToken(string token);
}

public class TokenParser : ITokenParser
{
    private readonly JsonWebTokenHandler _tokenHandler = new JsonWebTokenHandler();

    public bool CanReadToken(string token)
    {
        return _tokenHandler.CanReadToken(token);
    }

    public JsonWebToken ReadJsonWebToken(string token)
    {
        return _tokenHandler.ReadJsonWebToken(token);
    }
}
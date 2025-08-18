using System.Collections.Generic;
using System.Net.Http;

namespace YourGamesList.Api.Services.Twitch.Model.Requests;

public class TwitchAuthRequest
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public string GrantType { get; init; } = "client_credentials";

    public FormUrlEncodedContent ToFormUrlEncodedContent()
    {
        return new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new("client_id", ClientId),
            new("client_secret", ClientSecret),
            new("grant_type", GrantType)
        });
    }
}
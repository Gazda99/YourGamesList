using System;

namespace YourGamesList.Api.Services.Twitch.Model;

public class TwitchToken
{
    public required string AccessToken { get; set; }
    public required DateTimeOffset ExpiresOn { get; set; }
}
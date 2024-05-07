namespace YourGamesList.Services.Twitch.Exceptions;

public class TwitchAuthException : Exception
{
    public TwitchAuthException()
    {
    }

    public TwitchAuthException(string message)
        : base(message)
    {
    }

    public TwitchAuthException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
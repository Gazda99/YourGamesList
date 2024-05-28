using YourGamesList.Services.Igdb.Model;

namespace YourGamesList.Services.Igdb.Exceptions;

public class IgdbClientFetchDataException : IgdbClientException
{
    public string Endpoint { get; init; } = string.Empty;
    public string Request { get; init; } = "N/A";

    public IgdbClientFetchDataException(IgdbClientExceptionReason reason) : base(reason)
    {
    }

    public IgdbClientFetchDataException(IgdbClientExceptionReason reason, string message) : base(reason, message)
    {
    }

    public IgdbClientFetchDataException(IgdbClientExceptionReason reason, string message, Exception inner) : base(
        reason, message, inner)
    {
    }
}
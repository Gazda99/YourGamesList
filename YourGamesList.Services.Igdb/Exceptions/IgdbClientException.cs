using YourGamesList.Services.Igdb.Model;

namespace YourGamesList.Services.Igdb.Exceptions;

internal static class IgdbClientExceptionMessages
{
    public const string ResponseUnsuccessful = "Response from IGDB service is not successful.";
    public const string UnhandledException = "Unhandled exception occured when calling the IGDB service.";
    public const string ParsingResponseProblem = "Cannot parse response from the IGDB service.";
}

public class IgdbClientException : Exception
{
    public int? StatusCode { get; init; }
    public string ResponseContent { get; init; } = "N/A";


    public IgdbClientExceptionReason Reason { get; init; }

    public IgdbClientException(IgdbClientExceptionReason reason)
    {
        Reason = reason;
    }

    public IgdbClientException(IgdbClientExceptionReason reason, string message)
        : base(message)
    {
        Reason = reason;
    }

    public IgdbClientException(IgdbClientExceptionReason reason, string message, Exception inner)
        : base(message, inner)
    {
        Reason = reason;
    }
}
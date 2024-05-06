namespace YourGamesList.IgdbScraper.Services.IgdbClient.Exceptions;

public class IgdbClientException : Exception
{
    public string Endpoint { get; init; } = string.Empty;
    public int? StatusCode { get; init; }
    public string Request { get; init; } = "N/A";
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
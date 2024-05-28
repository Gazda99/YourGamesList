using YourGamesList.Services.Igdb.Model;

namespace YourGamesList.Services.Igdb.Exceptions;

public class IgdbClientWebhookException : IgdbClientException
{
    public string Operation { get; init; } = string.Empty;
    public string Method { get; init; } = "N/A";
    public long WebhookId { get; init; }
    public string Endpoint { get; init; } = "N/A";


    public IgdbClientWebhookException(IgdbClientExceptionReason reason) : base(reason)
    {
    }

    public IgdbClientWebhookException(IgdbClientExceptionReason reason, string message) : base(reason, message)
    {
    }

    public IgdbClientWebhookException(IgdbClientExceptionReason reason, string message, Exception inner) : base(reason,
        message, inner)
    {
    }
}
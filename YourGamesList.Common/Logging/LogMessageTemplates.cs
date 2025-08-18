using YourGamesList.Common.Refit;

namespace YourGamesList.Common.Logging;

public static class LogMessageTemplates
{
    public static string NetworkFailure(HttpFailureReason httpFailureReason, string destinationServiceName)
    {
        return $"Network error '{httpFailureReason.ToString()}' while making request to '{destinationServiceName}'.";
    }
}
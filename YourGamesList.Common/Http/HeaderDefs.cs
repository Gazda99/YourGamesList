namespace YourGamesList.Common.Http;

public static class HeaderDefs
{
    public const string HeaderCorrelationIdName = "ygl-correlation-id";
    public const string HeaderUserAgent = "user-agent";

    public static readonly (string, string) YglUserAgentHeaderTuple = (HeaderUserAgent, "ygl");
}
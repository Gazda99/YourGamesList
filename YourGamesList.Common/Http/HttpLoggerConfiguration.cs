using System.Collections.Generic;

namespace YourGamesList.Common.Http;

public class HttpLoggerConfiguration
{
    public Dictionary<string, string> CustomResponseHeadersToLog { get; set; } = [];
}
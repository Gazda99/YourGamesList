using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace YourGamesList.Common.Http;

[ExcludeFromCodeCoverage]
public class HttpLoggerConfiguration
{
    public Dictionary<string, string> CustomRequestHeadersToLog { get; set; } = [];
    public Dictionary<string, string> CustomResponseHeadersToLog { get; set; } = [];
}
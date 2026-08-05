using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace YourGamesList.TestsUtils;

public static class LogHelper
{
    public static void ReceivedLog(this ILogger logger, LogLevel level, string message, Predicate<Exception?>? ex = null)
    {
        ReceivedLog(logger, 1, level, message, ex);
    }

    public static void NotReceivedLog(this ILogger logger, LogLevel level, string message, Predicate<Exception?>? ex = null)
    {
        ReceivedLog(logger, 0, level, message, ex);
    }

    public static void ReceivedLog(this ILogger logger, int n, LogLevel level, string message, Predicate<Exception?>? ex = null)
    {
        logger.Received(n).Log(
            level,
            Arg.Any<EventId>(),
            Arg.Is<object>(x => ToStringOrEmpty(x) == message),
            ex == null ? Arg.Any<Exception?>() : Arg.Is<Exception?>(x => ex(x)),
            Arg.Any<Func<object, Exception?, string>>()
        );
    }

    public static void ReceivedLog(this ILogger logger, LogLevel level, Predicate<string> messagePredicate, Predicate<Exception?>? ex = null)
    {
        ReceivedLog(logger, 1, level, messagePredicate, ex);
    }


    public static void NotReceivedLog(this ILogger logger, LogLevel level, Predicate<string> messagePredicate, Predicate<Exception?>? ex = null)
    {
        ReceivedLog(logger, 0, level, messagePredicate, ex);
    }

    public static void ReceivedLog(this ILogger logger, int n, LogLevel level, Predicate<string> messagePredicate, Predicate<Exception?>? ex = null)
    {
        logger.Received(n).Log(
            level,
            Arg.Any<EventId>(),
            Arg.Is<object>(x => messagePredicate(ToStringOrEmpty(x))),
            ex == null ? Arg.Any<Exception?>() : Arg.Is<Exception?>(x => ex(x)),
            Arg.Any<Func<object, Exception?, string>>()
        );
    }

    public static void ReceivedLog(this ILogger logger, LogLevel level, string[] keywords, Predicate<Exception?>? ex = null)
    {
        ReceivedLog(logger, 1, level, keywords, ex);
    }

    public static void NotReceivedLog(this ILogger logger, LogLevel level, string[] keywords, Predicate<Exception?>? ex = null)
    {
        ReceivedLog(logger, 0, level, keywords, ex);
    }

    public static void ReceivedLog(this ILogger logger, int n, LogLevel level, string[] keywords, Predicate<Exception?>? ex = null)
    {
        logger.Received(n).Log(
            level,
            Arg.Any<EventId>(),
            Arg.Is<object>(x => x != null && CheckForKeywordsInLogMessage(x, keywords)),
            ex == null ? Arg.Any<Exception?>() : Arg.Is<Exception?>(x => ex(x)),
            Arg.Any<Func<object, Exception?, string>>()
        );
    }

    private static bool CheckForKeywordsInLogMessage(object logMessage, string[] keywords)
    {
        return keywords.All(keyword =>
        {
            if (string.IsNullOrEmpty(logMessage.ToString()))
            {
                return false;
            }

            return logMessage.ToString()!.Contains(keyword);
        });
    }

    public static void ReceivedBeginScope(this ILogger logger, Dictionary<string, object> scopes, int n = 1)
    {
        logger.Received(n).BeginScope(RequiredScopes(scopes.Select(x => (x.Key, x.Value))));
    }

    private static Dictionary<string, object> RequiredScopes(IEnumerable<(string key, object val)> scopes)
    {
        return Arg.Is<Dictionary<string, object>>(x => x != null && CheckScopes(x, scopes));
    }

    private static bool CheckScopes(Dictionary<string, object> scopesDict, IEnumerable<(string key, object val)> scopes)
    {
        foreach (var (key, expectedVal) in scopes)
        {
            if (!scopesDict.TryGetValue(key, out var foundValue))
            {
                TestContext.Out.WriteLine($"Cannot find key: '{key}' in scopes dictionary.");
                return false;
            }

            if (foundValue.ToString() != expectedVal.ToString())
            {
                TestContext.Out.WriteLine($"Found value: '{foundValue}' is not equal to expected: '{expectedVal}'.");
                return false;
            }
        }

        return true;
    }

    private static string ToStringOrEmpty(object? x)
    {
        return x?.ToString() ?? string.Empty;
    }
}